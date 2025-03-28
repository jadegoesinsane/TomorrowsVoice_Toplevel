document.addEventListener('DOMContentLoaded', function () {
	const backgroundColourInput = document.getElementById('BackgroundColour');
	const textColourInput = document.getElementById('TextColour');
	const borderColourInput = document.getElementById('BorderColour');

	function updateWCAGRatings() {
		const backgroundColour = backgroundColourInput.value;
		const textColour = textColourInput.value;

		fetch('/ColourScheme/GetRating', {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json'
			},
			body: JSON.stringify({ backgroundColour, textColour })
		})
			.then(response => response.json())
			.then(data => {
				document.getElementById('AA').textContent = data.normalAA;
				document.getElementById('AAA').textContent = data.normalAAA;
				document.getElementById('bigAA').textContent = data.bigAA;
				document.getElementById('bigAAA').textContent = data.bigAAA;

				document.getElementById('AA').className = 'badge ' + (data.normalAA === 'FAIL' ? 'bg-danger' : 'bg-success');
				document.getElementById('AAA').className = 'badge ' + (data.normalAAA === 'FAIL' ? 'bg-danger' : 'bg-success');
				document.getElementById('bigAA').className = 'badge ' + (data.bigAA === 'FAIL' ? 'bg-danger' : 'bg-success');
				document.getElementById('bigAAA').className = 'badge ' + (data.bigAAA === 'FAIL' ? 'bg-danger' : 'bg-success');

				document.getElementById('ratio').textContent = data.ratio;
				document.getElementById('ratio-container').className = 'col-auto rounded-3 align-content-center border ' + (data.status === "PASS" ? 'border-success' : data.status === "FAIL" ? 'border-danger' : 'border-secondary');
				document.getElementById('normalExample').style.borderLeftColor = backgroundColour;
				document.getElementById('fcBg').style.backgroundColor = backgroundColour;
				document.getElementById('fcBg').style.border = "2px solid " + borderColourInput.value;
				document.getElementById('fcTxt').style.color = textColour;
			});
	}

	backgroundColourInput.onchange = updateWCAGRatings;
	textColourInput.onchange = updateWCAGRatings;
	borderColourInput.onchange = updateWCAGRatings;
});