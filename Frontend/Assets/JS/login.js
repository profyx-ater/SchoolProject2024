window.addEventListener("load", handleLoad);

import {displayError} from "../Util/ErrorHandeling.js";

function handleLoad() {
    let email = document.getElementById("username");
    email.value = sessionStorage.getItem("email") ? sessionStorage.getItem("email") : null;

    let inloggen = document.getElementById("inlogButton");
    inloggen.addEventListener('click', login);
}

function login(event) {
    event.preventDefault();
    let inputEmail = document.getElementById("username").value;
    let inputPassword = document.getElementById("password").value;
    let emailError = document.getElementById("emailError")
    let passwordError = document.getElementById("passwordError")
    emailError.textContent = "";
    passwordError.textContent = "";

    if (inputEmail === '' || inputEmail === null) {
        emailError.textContent = 'Please enter your email.';
        return;
    }
    if (inputPassword === '' || inputPassword === null){
        passwordError.textContent = 'Please enter your password.';
        return;
    }
    if (inputPassword.length < 6){
        passwordError.textContent = 'Password must be at least 6 characters.';
        return;
    }
    let myData = {
        email: inputEmail,
        password: inputPassword
    };

    const url = 'https://localhost:5051/api/Authentication/token'
    let errorOutput = document.getElementById("errorOutput");

    fetch(url, {
        method: "POST",
        body: JSON.stringify(myData),
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
        }
    })
        .then(response => {
            if (response.status === 401) {
                throw 'Unauthorized: The password and/or email are incorrect. Please try again.';
            } else {
                return response.json();
            }
        })
        .then(data => {
            // Gegevens opslaan in sessionStorage
            let {token, user: {id, email, warriorName}} = data;

            sessionStorage.setItem("token", token);
            sessionStorage.setItem("Id", id);
            sessionStorage.setItem("Email", email);
            sessionStorage.setItem("WarriorName", warriorName);

            // nieuwe pagina openen
            window.location.href = "./lobby.html";
        })
        .catch(error => {
            displayError(error, errorOutput)
        })
}

