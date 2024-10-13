const setImagePreview = (event) => {
    const file = event.target.files[0];

    if (file && file.type.startsWith("image/")) {
        const reader = new FileReader();

        reader.onloadend = () => {
            document.getElementById("profileImage").src = reader.result;
        };

        reader.readAsDataURL(file);
    }
}