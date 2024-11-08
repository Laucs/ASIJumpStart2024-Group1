const setImagePreview = (event) => {
    const file = event.target.files[0];

    if (file && file.type.startsWith("image/")) {
        const reader = new FileReader();

        reader.onloadend = () => {
            document.getElementById("profileImage").src = reader.result; 
        };

        const formData = new FormData();
        formData.append("ProfilePicture", file);

        fetch('/Pref/SaveImagePath', {
            method: 'POST',
            body: formData 
        })
            .then(response => {
                if (!response.ok) throw new Error("Network response was not ok");
                return response.json();
            })
            .then(data => {
                console.log("Image file path saved:", data.filePath);
            })
            .catch(error => console.error('Error:', error));

        reader.readAsDataURL(file);
    }
}
