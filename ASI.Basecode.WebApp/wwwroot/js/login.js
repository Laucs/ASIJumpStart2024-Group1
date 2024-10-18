var errMessage = '@TempData["ErrorMessage"]';
var regSuccess = '@TempData["RegSuccess"]';
var invalidCred = '@TempData["InvalidCred"]';
var emailVerified = '@TempData["EmailVerified"]';
var emailNotVerified = '@TempData["EmailNotVerified"]';

if (errMessage) {
    toastr.error(errMessage);
}
if (regSuccess) {
    toastr.success(regSuccess);
}
if (invalidCred) {
    toastr.error(invalidCred);
}
if (emailVerified) {
    toastr.success(emailVerified);
}
if (emailNotVerified) {
    toastr.error(emailNotVerified);
}
