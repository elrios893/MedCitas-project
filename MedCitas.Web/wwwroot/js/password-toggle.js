// Toggle password visibility
document.addEventListener('DOMContentLoaded', function () {
    const togglePassword = document.getElementById('togglePassword');
    const toggleConfirmPassword = document.getElementById('toggleConfirmPassword');

    if (togglePassword) {
        togglePassword.addEventListener('click', function () {
            const passwordInput = document.getElementById('password');
            const icon = this.querySelector('i');

            if (passwordInput.type === 'password') {
                passwordInput.type = 'text';
                icon.classList.remove('bi-eye');
                icon.classList.add('bi-eye-slash');
            } else {
                passwordInput.type = 'password';
                icon.classList.remove('bi-eye-slash');
                icon.classList.add('bi-eye');
            }
        });
    }

    if (toggleConfirmPassword) {
        toggleConfirmPassword.addEventListener('click', function () {
            const confirmPasswordInput = document.getElementById('confirmarPassword');
            const icon = this.querySelector('i');

            if (confirmPasswordInput.type === 'password') {
                confirmPasswordInput.type = 'text';
                icon.classList.remove('bi-eye');
                icon.classList.add('bi-eye-slash');
            } else {
                confirmPasswordInput.type = 'password';
                icon.classList.remove('bi-eye-slash');
                icon.classList.add('bi-eye');
            }
        });
    }

    // Validar que las contraseñas coincidan
    const confirmarPassword = document.getElementById('confirmarPassword');
    if (confirmarPassword) {
        confirmarPassword.addEventListener('input', function () {
            const password = document.getElementById('password').value;
            const confirmPassword = this.value;

            if (password !== confirmPassword) {
                this.setCustomValidity('Las contraseñas no coinciden');
            } else {
                this.setCustomValidity('');
            }
        });
    }
});