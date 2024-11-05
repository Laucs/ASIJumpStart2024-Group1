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
        myLineChart;
    })
    .catch(error => console.error('Error fetching chart data:', error));
