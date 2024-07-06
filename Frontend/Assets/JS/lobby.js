window.addEventListener("load", handleLoad)
import {displayError} from '../Util/ErrorHandeling.js'
import {fetchData} from "../Util/FetchHandeling.js";

const profileButton = document.getElementById('profileButton');
const profileInfo = document.getElementById('profileInfo');
let Newtable = document.getElementById("NewTableButton");
let errorOutput = document.getElementById("errorOutput");
let tableContainer = document.getElementById('table-container') || document.createElement('div');


const token = sessionStorage.getItem("token");
const WarriorName = sessionStorage.getItem("WarriorName");
const Email = sessionStorage.getItem("Email");

const headers = {
    'Accept': 'application/json',
    'Content-Type': 'application/json',
    'Authorization': 'Bearer ' + token
}


function handleLoad() {
    document.getElementById('inputWariorName').textContent = WarriorName;
    document.getElementById('inputEmail').textContent = Email;
    document.getElementById('WarriorName').textContent = WarriorName;

    profileButton.addEventListener("click", profileButtonClick);

    const logout = document.getElementById("logout");
    logout.addEventListener("click", logoutClick);

    Newtable.addEventListener("click", NewtableClick);
    showTables()
}

function profileButtonClick() {
    if (profileInfo.style.display === 'none' || profileInfo.style.display === '') {
        profileInfo.style.display = 'block';
    } else {
        profileInfo.style.display = 'none';
    }
}

function logoutClick() {
    sessionStorage.clear();
    window.location.replace("./index.html");
}

async function NewtableClick() {
    const myData = {
        "numberOfPlayers": 2,
        "playerMatSize": 5,
        "moveCardSet": 0
    };
    const url = 'https://localhost:5051/api/Tables'
    const options = {method: "POST", body: JSON.stringify(myData), headers: headers};

    try {
        const request = await fetchData(url, options);
        let {id, ownerPlayerId} = request
        sessionStorage.setItem("tableId", id)
        sessionStorage.setItem("ownerPlayerId", ownerPlayerId)
        window.location.replace("./game.html")
    } catch (error) {
        displayError(error, errorOutput);
    }
}

function showTables() {
    const url = 'https://localhost:5051/api/Tables/with-available-seats';
    fetch(url, {
        headers: headers
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(tables => {
            if (Array.isArray(tables) && tables.length > 0) {
                createTables(tables);
            } else {
                tableContainer.innerHTML = '';
            }
        })
        .catch(error => {
            console.error('Er is een fout opgetreden bij het ophalen van de tabellen:', error);
        });
}

function createTables(tables) {
    tableContainer.setAttribute('id', 'table-container');
    tableContainer.innerHTML = '';
    tableContainer.classList.add('container');

    tables.forEach((table, index) => {
        const tableElement = createTableImage(table, index);
        tableContainer.appendChild(tableElement);
    });

    document.body.appendChild(tableContainer);
}

function createTableImage(table) {
    // Maak een div-element
    const {id, seatedPlayers: [{name}]} = table;
    let tableDiv = document.createElement('div');
    tableDiv.classList.add('tableDiv');

    // Voeg eerste regel toe: "Player 1"
    let firstLine = document.createElement('p');
    firstLine.innerHTML = `Seated player:<br>${name}`;
    tableDiv.appendChild(firstLine);

    // Voeg afbeelding toe
    let boardImage = document.createElement('img');
    boardImage.src = "Assets/Image/playTable.png";
    boardImage.alt = "board";
    boardImage.onload = function () {
        // Stel de breedte van de div in op basis van de breedte van de afbeelding
        tableDiv.style.width = boardImage.width + "px";
    };
    tableDiv.appendChild(boardImage);

    // Voeg derde regel toe: "Join" knop
    let joinButton = makeButton(id);
    joinButton.classList.add('joinButton');
    tableDiv.appendChild(joinButton);

    return tableDiv;
}

function makeButton(tableId) {
    let joinButton = document.createElement('button');
    joinButton.textContent = "Join";
    joinButton.name = "join";
    joinButton.dataset.tableId = tableId;
    joinButton.addEventListener("click", () => {
        JoinExistingGame(joinButton.dataset.tableId);
    });

    return joinButton;

}

async function JoinExistingGame(tableid) {
    const url = `https://localhost:5051/api/Tables/${tableid}/join`;
    const options = {method: "POST", headers: headers};

    try {
        await fetchData(url, options);
        sessionStorage.setItem("tableId", tableid);
        window.location.replace("./game.html")
    } catch (error) {
        displayError(error, errorOutput);
    }
}

setInterval(function () {
    showTables();
}, 2000);