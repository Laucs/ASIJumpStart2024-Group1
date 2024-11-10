document.addEventListener('DOMContentLoaded', function () {
    const togglePasswordCheckbox = document.getElementById('togglePasswordChange');
    const passwordFields = document.querySelectorAll('#passwordFields input[type="password"]');

    // Function to toggle the disabled state for password fields
    function togglePasswordFields() {
        passwordFields.forEach(input => {
            input.disabled = !togglePasswordCheckbox.checked;  // Enable or disable input based on checkbox
        });
    }

    // Initially set the fields based on the checkbox state
    togglePasswordFields();

    // Listen for changes to the checkbox and update the state of the password fields
    togglePasswordCheckbox.addEventListener('change', function () {
        togglePasswordFields();
    });
});