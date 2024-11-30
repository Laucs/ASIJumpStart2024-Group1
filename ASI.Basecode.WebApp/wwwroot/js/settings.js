document.addEventListener('DOMContentLoaded', function () {
    const togglePasswordCheckbox = document.getElementById('togglePasswordChange');
    const passwordFields = document.querySelectorAll('#passwordFields input[type="password"]');
    const newPassSubmitBtn = document.getElementById('submitBtn');

    const usernameInput = document.getElementById("usernameInput");
    const newUNEditButton = document.getElementById("newUNEditButton");
    const newUNSubmitButton = document.getElementById("newUNSubmitButton");
    const newUNCancelButton = document.getElementById("newUNCancelButton");

    const emailInput = document.getElementById("emailInput");
    const newEmailEditButton = document.getElementById("newEmailEditButton");
    const newEmailSubmitButton = document.getElementById("newEmailSubmitButton");
    const newEmailCancelButton = document.getElementById("newEmailCancelButton");

    var originalUsername = "";
    var originalEmail = "";

    function togglePasswordFields() {
        passwordFields.forEach(input => {
            input.disabled = !togglePasswordCheckbox.checked;  
            input.value = "";
        });
        newPassSubmitBtn.hidden = !togglePasswordCheckbox.checked;
    }

    togglePasswordCheckbox.addEventListener('change', function () {
        togglePasswordFields();
    });


    newUNEditButton.addEventListener('click', () => {
        originalUsername = usernameInput.value;
        usernameInput.removeAttribute("readonly");
        usernameInput.removeAttribute("disabled");
        usernameInput.focus();
        usernameInput.select();
        usernameInput.title = "Please enter a valid username."; 
        newUNEditButton.setAttribute("hidden", true);
        newUNSubmitButton.removeAttribute("hidden");
        newUNCancelButton.removeAttribute("hidden");
    });

    newUNCancelButton.addEventListener('click', () => {
        usernameInput.setAttribute("readonly", true);
        usernameInput.setAttribute("disabled", true);
        newUNSubmitButton.setAttribute("hidden", true);
        newUNCancelButton.setAttribute("hidden", true);  
        newUNEditButton.removeAttribute("hidden");
        usernameInput.value = originalUsername;
   
    });

    newEmailEditButton.addEventListener('click', () => {
        newEmailEditButton.setAttribute("hidden", false);
        newEmailSubmitButton.removeAttribute("hidden");
        newEmailCancelButton.removeAttribute("hidden");
        originalEmail = emailInput.value;
        emailInput.removeAttribute("readonly");
        emailInput.removeAttribute("disabled");
        emailInput.title = "Please enter a valid email address"; 
        emailInput.focus();
        emailInput.select();
    });

    newEmailCancelButton.addEventListener('click', () => {
        emailInput.setAttribute("readonly", true);
        emailInput.setAttribute("disabled", true);
        newEmailSubmitButton.setAttribute("hidden", true);
        newEmailCancelButton.setAttribute("hidden", true);
        newEmailEditButton.removeAttribute("hidden");
        emailInput.value = originalEmail;

    });      

});