window.addEventListener("load", handleLoad);
import {displayError} from "../Util/ErrorHandeling.js";

function handleLoad() {
    let registratiebtn = document.getElementById('registreerbtn');
    registratiebtn.addEventListener("click", RegistreerControle);
    let gainlog = document.getElementById("galoginbtn")
    gainlog.addEventListener('click', naarlogin)

}

function RegistreerControle(event) {
    event.preventDefault();
    let inputMail = document.getElementById("email").value;
    let inputwarior = document.getElementById("wariornaam").value;
    let inputWW = document.getElementById("wachtwoord").value;
    let inputWWVeri = document.getElementById("wachtwoordVerificatie").value;
    let errorOutput = document.getElementById("errors")
    errorOutput.style.color = "Red";

    const userData = {
        wariorName: inputwarior,
        email: inputMail,
        password: inputWW
    };
    const requestOptions = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(userData)
    };
    if (inputWW !== inputWWVeri) {
        errorOutput.textContent = "The password fields do not match.";
    } else {
    fetch('https://localhost:5051/api/Authentication/register', requestOptions)
        .then(response => {
             if (response.status === 400) {
                return response.json()
                    .then(error => {
                    displayError(error, errorOutput);
                });
            } else {
                errorOutput.style.color = "Green";
                errorOutput.textContent = "Registration successful! You will immediately be redirected to the login page."
                sessionStorage.setItem('email', inputMail)
                setTimeout(() => {
                    window.location.replace("./index.html");
                }, 3000);
            }
        })
}}

function naarlogin(event) {
    event.preventDefault();
    window.location.replace("./index.html");
}