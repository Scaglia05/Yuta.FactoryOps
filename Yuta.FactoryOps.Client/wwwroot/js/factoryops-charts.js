// Interop de gráficos (Chart.js) para o painel FactoryOps.
// Mantém as instâncias de Chart.js vivas entre chamadas do Blazor, já que o componente
// C# não guarda referências a objetos JS diretamente.
window.fops = (function () {
    let vibChart, tempChart, fftChart, histChart, availChart, stopChart, cycleChart, energyChart, energyTrendChart;

    const baseOpts = () => ({
        responsive: true, maintainAspectRatio: false, animation: { duration: 0 },
        plugins: { legend: { display: false } },
        scales: {
            x: { display: false, grid: { display: false } },
            y: { grid: { color: 'rgba(42,30,72,.8)' }, ticks: { color: '#6a5a80', font: { family: 'Space Mono', size: 9 } } }
        }
    });

    function destroy(chart) { if (chart) chart.destroy(); return undefined; }

    function drawOeeRing(canvasId, value, color) {
        const cvs = document.getElementById(canvasId);
        if (!cvs) return;
        const ctx = cvs.getContext('2d');
        const cx = 55, cy = 55, r = 44, sw = 9;
        ctx.clearRect(0, 0, 110, 110);
        ctx.beginPath(); ctx.arc(cx, cy, r, 0, Math.PI * 2);
        ctx.strokeStyle = '#1b1532'; ctx.lineWidth = sw; ctx.stroke();
        const end = -Math.PI / 2 + (Math.PI * 2) * (value / 100);
        ctx.beginPath(); ctx.arc(cx, cy, r, -Math.PI / 2, end);
        ctx.strokeStyle = color; ctx.lineWidth = sw; ctx.lineCap = 'round'; ctx.stroke();
    }

    function initTrendCharts(vibCanvasId, tempCanvasId, labels, vibData, tempData, vibColor, tempColor, vibMax, tempMin, tempMax) {
        vibChart = destroy(vibChart);
        tempChart = destroy(tempChart);
        const vibEl = document.getElementById(vibCanvasId);
        const tempEl = document.getElementById(tempCanvasId);
        if (!vibEl || !tempEl) return;
        vibChart = new Chart(vibEl, {
            type: 'line',
            data: { labels, datasets: [{ data: vibData, borderColor: vibColor, borderWidth: 1.5, pointRadius: 0, fill: true, backgroundColor: vibColor + '22', tension: .4 }] },
            options: { ...baseOpts(), scales: { ...baseOpts().scales, y: { ...baseOpts().scales.y, suggestedMin: 0, suggestedMax: vibMax } } }
        });
        tempChart = new Chart(tempEl, {
            type: 'line',
            data: { labels, datasets: [{ data: tempData, borderColor: tempColor, borderWidth: 1.5, pointRadius: 0, fill: true, backgroundColor: tempColor + '22', tension: .4 }] },
            options: { ...baseOpts(), scales: { ...baseOpts().scales, y: { ...baseOpts().scales.y, suggestedMin: tempMin, suggestedMax: tempMax } } }
        });
    }

    function pushTrendData(vibValue, tempValue, maxPts) {
        if (!vibChart || !tempChart) return;
        const vd = vibChart.data.datasets[0].data;
        const td = tempChart.data.datasets[0].data;
        const lb = vibChart.data.labels;
        if (vd.length >= maxPts) { vd.shift(); td.shift(); lb.shift(); }
        vd.push(vibValue); td.push(tempValue); lb.push('');
        vibChart.update('none'); tempChart.update('none');
    }

    function initFftChart(canvasId, labels, amps, colors) {
        fftChart = destroy(fftChart);
        const el = document.getElementById(canvasId);
        if (!el) return;
        fftChart = new Chart(el, {
            type: 'bar',
            data: { labels, datasets: [{ data: amps, backgroundColor: colors, borderWidth: 0, barPercentage: .9 }] },
            options: {
                responsive: true, maintainAspectRatio: false, animation: { duration: 400 },
                plugins: { legend: { display: false } },
                scales: {
                    x: { ticks: { color: '#6a5a80', font: { family: 'Space Mono', size: 8 }, maxTicksLimit: 12 }, grid: { color: 'rgba(42,30,72,.4)' } },
                    y: { ticks: { color: '#6a5a80', font: { family: 'Space Mono', size: 9 } }, grid: { color: 'rgba(42,30,72,.4)' }, title: { display: true, text: 'g', color: '#6a5a80', font: { size: 9 } } }
                }
            }
        });
    }

    function initHistoryCharts(days, datasets, availData, stopLabels, stopData, cycleLabels, cycleData) {
        histChart = destroy(histChart);
        availChart = destroy(availChart);
        stopChart = destroy(stopChart);
        cycleChart = destroy(cycleChart);
        const bo = () => ({
            responsive: true, maintainAspectRatio: false, animation: { duration: 600 },
            plugins: { legend: { display: false } },
            scales: {
                x: { ticks: { color: '#6a5a80', font: { size: 9 } }, grid: { color: 'rgba(42,30,72,.4)' } },
                y: { ticks: { color: '#6a5a80', font: { family: 'Space Mono', size: 9 }, callback: v => v + '%' }, grid: { color: 'rgba(42,30,72,.4)' }, min: 50, max: 100 }
            }
        });
        const histEl = document.getElementById('histChart');
        if (histEl) {
            histChart = new Chart(histEl, {
                type: 'line', data: { labels: days, datasets },
                options: { ...bo(), plugins: { legend: { display: true, labels: { color: '#a898cc', font: { size: 10 }, boxWidth: 10, boxHeight: 10 } } } }
            });
        }
        const availEl = document.getElementById('availChart');
        if (availEl) {
            availChart = new Chart(availEl, {
                type: 'bar',
                data: { labels: days, datasets: [{ data: availData, backgroundColor: 'rgba(123,47,190,.6)', borderWidth: 0, borderRadius: 3 }] },
                options: { ...bo(), scales: { ...bo().scales, y: { ...bo().scales.y, min: 70 } } }
            });
        }
        const stopEl = document.getElementById('stopChart');
        if (stopEl) {
            stopChart = new Chart(stopEl, {
                type: 'doughnut',
                data: { labels: stopLabels, datasets: [{ data: stopData, backgroundColor: ['#8b1515', '#7b2fbe', '#c8941a', '#1a8c4e'], borderWidth: 0 }] },
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: true, labels: { color: '#a898cc', font: { size: 9 }, boxWidth: 8 } } } }
            });
        }
        const cycleEl = document.getElementById('cycleChart');
        if (cycleEl) {
            cycleChart = new Chart(cycleEl, {
                type: 'bar',
                data: { labels: cycleLabels, datasets: [{ data: cycleData, backgroundColor: ['rgba(123,47,190,.7)', 'rgba(200,148,26,.7)', 'rgba(42,30,72,.7)'], borderWidth: 0, borderRadius: 3 }] },
                options: {
                    responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } },
                    scales: { x: { ticks: { color: '#6a5a80', font: { size: 9 } }, grid: { display: false } }, y: { ticks: { color: '#6a5a80', font: { family: 'Space Mono', size: 9 } }, grid: { color: 'rgba(42,30,72,.4)' } } }
                }
            });
        }
    }

    function initEnergyCharts(assetLabels, powerData, colors, hours, trendData) {
        energyChart = destroy(energyChart);
        energyTrendChart = destroy(energyTrendChart);
        const bo = {
            responsive: true, maintainAspectRatio: false, animation: { duration: 400 }, plugins: { legend: { display: false } },
            scales: { x: { ticks: { color: '#6a5a80', font: { size: 9 } }, grid: { color: 'rgba(42,30,72,.4)' } }, y: { ticks: { color: '#6a5a80', font: { family: 'Space Mono', size: 9 }, callback: v => v + 'kW' }, grid: { color: 'rgba(42,30,72,.4)' } } }
        };
        const energyEl = document.getElementById('energyChart');
        if (energyEl) {
            energyChart = new Chart(energyEl, {
                type: 'bar',
                data: { labels: assetLabels, datasets: [{ data: powerData, backgroundColor: colors, borderWidth: 0, borderRadius: 4 }] },
                options: bo
            });
        }
        const trendEl = document.getElementById('energyTrendChart');
        if (trendEl) {
            energyTrendChart = new Chart(trendEl, {
                type: 'line',
                data: { labels: hours, datasets: [{ data: trendData, borderColor: '#7b2fbe', borderWidth: 1.5, pointRadius: 0, fill: true, backgroundColor: 'rgba(123,47,190,.15)', tension: .4 }] },
                options: { ...bo, scales: { ...bo.scales, x: { ...bo.scales.x, ticks: { ...bo.scales.x.ticks, maxTicksLimit: 8 } } } }
            });
        }
    }

    function scrollToBottom(elementId) {
        const el = document.getElementById(elementId);
        if (el) el.scrollTop = el.scrollHeight;
    }

    function downloadTextFile(filename, content) {
        const blob = new Blob([content], { type: 'text/plain' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url; a.download = filename; a.click();
        URL.revokeObjectURL(url);
    }

    function getTheme() {
        try { return localStorage.getItem('fops-theme'); } catch { return null; }
    }

    function setTheme(theme) {
        try { localStorage.setItem('fops-theme', theme); } catch { /* ignore */ }
    }

    return {
        drawOeeRing, initTrendCharts, pushTrendData, initFftChart, initHistoryCharts, initEnergyCharts,
        scrollToBottom, downloadTextFile, getTheme, setTheme
    };
})();
