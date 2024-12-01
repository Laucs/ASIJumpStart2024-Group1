fetch('/api/chart-data')
    .then(response => response.json())
    .then(chartData => {
        var ctx = document.getElementById('lineChart').getContext('2d');
        var myLineChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: chartData.labels,
                datasets: chartData.datasets
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });

        if (chartData.previousWeekPercentage !== undefined) {
        const percentageLabel = document.getElementById('previousWeekPercentLabel');
        const svgIcon = document.getElementById('svgIcons');
        const percentage = chartData.previousWeekPercentage;

        percentageLabel.textContent = `${percentage}%`;

        percentageLabel.classList.remove('text-red-600', 'text-gray-400', 'text-[#00FD87]');

        if (percentage < 0) {
            percentageLabel.classList.add('text-[#00FD87]'); 
            svgIcon.src = '/img/Down.svg';
        } else if (percentage == 0 || percentage == 100) {
            percentageLabel.classList.add('text-gray-400');
            svgIcon.src = '/img/Line.svg';
        } else {
            percentageLabel.classList.add('text-red-600');
            svgIcon.src = '/img/Up.svg';
        }
    }
    })
    .catch(error => console.error('Error fetching chart data:', error));
