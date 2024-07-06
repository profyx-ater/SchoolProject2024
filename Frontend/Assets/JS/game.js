'use strict'

import {displayError} from '../Util/ErrorHandeling.js'
import {fetchData} from '../Util/FetchHandeling.js'

window.addEventListener("load", handleLoad)

const headers = {
    'Accept': 'application/json',
    'Content-Type': 'application/json',
    'Authorization': 'Bearer ' + sessionStorage.getItem("token")
};

let idOwnerPlayer;
let nameOtherPlayer = "";
let tableData;
let idPlayerToPlay;
let playersPlaying;
let numberOfPlayers;
let selectedPawn;
let previousSelectedPawn;
let selectedCard = null;
let previousSelectedCard;
let idWinner;
let methodeUsedToWin;
let disableEventSquare = false;
let gameId;
let gameStarted = false;
let firstTime = true;
let playMat;
let oldSquare;
let eventRegistry = new Map();

// Session Storage
let currentUserId = sessionStorage.getItem("Id");
let tableId = sessionStorage.getItem("tableId");

// DOM Elementen
const leaveGameBtn = document.getElementById("leaveGameBtn");
const skipMoveBtn = document.getElementById("skipMoveBtn");
let playerLeftElement = document.getElementById("PlayerLeft");
let intro = document.getElementById("intro");
let intro2 = document.getElementById("intro2")
let showPlayerOutput = document.getElementById("showPlayers")
let startgameBtn = document.getElementById("StartGameBtn");
let errorOutput = document.getElementById("ErrorOutput");
let board = document.getElementById("Board");
let startingplayer = document.getElementById("startingPlayer");
let startingplayer2 = document.getElementById("startingPlayer2");

async function handleLoad() {
    leaveGameBtn.addEventListener("click", LeaveGame);
    await ShowPlayers();
    introductionMessage();
    startgameBtn.addEventListener("click", StartGame);
}

function introductionMessage() {
    if (idOwnerPlayer === currentUserId) {
        intro.textContent = "Waiting for another player to join the game.";
    } else {
        intro.textContent = "Wait until the owner starts the game.";
    }
}

async function LeaveGame() {
    const url = `https://localhost:5051/api/Tables/${tableId}/leave`
    const options = {method: "POST", headers}

    try {
        await fetchData(url, options);
        window.location.replace("./lobby.html");

    } catch (error) {
        displayError(error, errorOutput);
    }
}

async function ShowPlayers() {
    const url = `https://localhost:5051/api/Tables/${tableId}`;
    const options = {method: "GET", headers: headers};

    try {
        const data = await fetchData(url, options);
        const {seatedPlayers, ownerPlayerId, hasAvailableSeat} = data;
        tableData = data;
        gameId = data.gameId;
        numberOfPlayers = data.seatedPlayers.length;

        if (gameId !== "00000000-0000-0000-0000-000000000000") {
            intro.textContent = "";
            intro2.textContent = "";
        }
        if (numberOfPlayers === 1) {
            if (nameOtherPlayer !== "") {
                PlayerLeft();
            } else {
                introductionMessage();
            }
        }
        if (numberOfPlayers === 2) {
            intro2.textContent = "";
        }

        idOwnerPlayer = ownerPlayerId;

        const playerNames = seatedPlayers.map(player => player.name);
        playerNames[0] += `, Color: ${seatedPlayers[0].color}`;

        if (numberOfPlayers === 2) {
            playerNames[1] += `, Color: ${seatedPlayers[1].color}`;
        }
        showPlayerOutput.innerHTML = playerNames.join('<br>');

        if (numberOfPlayers === 2) {
            playerLeftElement.textContent = "";
            if (seatedPlayers[0].id === currentUserId) {
                nameOtherPlayer = seatedPlayers[1].name;
            } else {
                nameOtherPlayer = seatedPlayers[0].name;
            }
        }

        if (idOwnerPlayer === currentUserId && !hasAvailableSeat && gameId === "00000000-0000-0000-0000-000000000000") {
            intro.textContent = "All seats are taken. You can start the game";
        }

    } catch (error) {
        displayError(error, errorOutput);
    }
}

function PlayerLeft() {
    playerLeftElement.textContent = `Player ${nameOtherPlayer} has left the game`;
    nameOtherPlayer = "";
    setTimeout(() => {
        playerLeftElement.textContent = ""
    }, 5000);
    if (!gameStarted && idOwnerPlayer !== currentUserId) {
        intro.textContent = "The owner has left the game. You are the new owner."
    } else if (gameStarted) {
        intro.textContent = "The other player left! You won the game :D";
        intro2.textContent = "You will be redirected to the lobby in 5 seconds";

        setTimeout(() => LeaveGame(), 5000);
    }
}

async function StartGame() {
    const url = `https://localhost:5051/api/Tables/${tableId}/start-game`;
    const options = {method: "POST", headers: headers};

    try {
        const data = await fetchData(url, options);
        gameId = data.gameId;

        intro.textContent = "";
        intro2.textContent = "";

        CreateTable();
        startgameBtn.style.display = "none";
        CurrentPlayerToOrient();

        await SetPawnsOnMat();
        gameStarted = true;
    } catch (error) {
        displayError(error, errorOutput);
    }
}

function CurrentPlayerToOrient() {
    const {direction: Player1Direction} = tableData.seatedPlayers[0];
    const {direction: Player2Direction} = tableData.seatedPlayers[1];

    if (Player1Direction && Player2Direction) {
        const currentPlayerDirection = currentUserId === tableData.seatedPlayers[0].id ? Player1Direction : Player2Direction;
        updateBoardOrientation(currentPlayerDirection);
    }
}

function CreateTable() {
    for (let row = 0; row < 5; row++) {
        for (let column = 4; column >= 0; column--) {
            let square = document.createElement("div");
            square.className = "square";
            square.id = `square-${row}-${column}`;
            board.appendChild(square);
        }
    }
    skipMoveBtn.addEventListener("click", skipMove);
    skipMoveBtn.style.visibility = "visible";
}

function ClearBoard() {
    const squares = document.querySelectorAll('[id^="square-"]');
    squares.forEach(square => {
        square.innerHTML = ''; // Remove all children from the square
    });
}

async function SetPawnsOnMat() {
    const url = `https://localhost:5051/api/Games/${gameId}`;
    const options = {method: "GET", headers: headers};
    ClearBoard();
    if (gameId !== "00000000-0000-0000-0000-000000000000") {
        try {
            const game = await fetchData(url, options);
            console.log(game);

            console.log(eventRegistry);
            playersPlaying = game.players;
            idPlayerToPlay = game.playerToPlayId;
            idWinner = game.winnerPlayerId;
            methodeUsedToWin = game.winnerMethod;
            playMat = game.playMat

            PlayersTurn(playersPlaying);
            await possibleMovesOpponent();
            if (idWinner !== "00000000-0000-0000-0000-000000000000") {
                thereIsWinner()
            }
            playersPlaying.forEach(player => {
                const {row, column} = player.school.templeArchPosition;
                const templeSquareId = `square-${row}-${column}`;
                const templeSquare = document.getElementById(templeSquareId);
                templeSquare.style.backgroundColor = player.color;

                for (let row = 0; row < 5 ; row++){
                    for (let column = 0; column < 5; column++){
                        const pawn = game.playMat.grid[row][column];
                        if (pawn) {
                            const pawnSquareId = `square-${row}-${column}`;
                            const square = document.getElementById(pawnSquareId);

                            if (idPlayerToPlay === currentUserId && pawn.OwnerId === currentUserId) {
                                if (!eventRegistry.has(square.id)){
                                    enableEvents(square, pawn, ()=> {selectingPawn(pawn, square)});
                                }
                            } else {
                                disableEvents(square);
                            }

                            if (square.children.length === 0) {
                                const color = (playersPlaying.find(player => player.id === pawn.OwnerId)).color;
                                const pawnElement = CreatePawnElement(pawn, color);
                                square.appendChild(pawnElement);
                            }
                        }
                    }
                }

            });
            CreateCards(game);

        } catch (error) {
            console.error('Error fetching data:', error);
        }
    }
}
function enableEvents(target, data, func){
    const handler = createEventHandler(data, func);
    target.addEventListener('click', handler);
    target._clickHandler = handler;

    if(!eventRegistry.has(target.id)){
        eventRegistry.set(target.id, "Running");
    }
}


function disableEvents(target){
    const handler = target._clickHandler;
    if (handler) {
        target.removeEventListener('click', handler);
        delete target._clickHandler;
        eventRegistry.delete(target.id);
        console.log(`Event disabled for ${target.id}`);
    }
}

function createEventHandler(data, func){
    return func;
}

function PlayersTurn(players) {
    let cardsNotOnMove;
    let cardOnMove;

    if (players[0].id === idPlayerToPlay) {
        if (idPlayerToPlay === currentUserId) {
            startingplayer.textContent = `your turn`;
        } else {
            startingplayer.textContent = `${players[0].name}'s turn`;
        }

        startingplayer.style.color = players[0].color;
        cardsNotOnMove = document.getElementById("OppesitePlayerCards");
        cardOnMove = document.getElementById('PlayerCards');

    } else {
        if (idPlayerToPlay === currentUserId) {
            startingplayer.textContent = `your turn`;
        } else {
            startingplayer.textContent = `${players[1].name}'s turn`;
        }

        startingplayer.style.color = players[1].color;
        cardsNotOnMove = document.getElementById("PlayerCards");
        cardOnMove = document.getElementById('OppesitePlayerCards')
    }

    cardsNotOnMove.style.opacity = 0.5;
    cardOnMove.style.opacity = 1;
}

function CreatePawnElement(pawn, color) {
    const pawnElement = document.createElement("img");
    const pawnType = pawn.Type === 0 ? "Master" : "Student";
    pawnElement.id = pawn.Id;
    pawnElement.src = `./Assets/Image/${pawnType}/${color}.png`;
    pawnElement.alt = pawnType;
    pawnElement.classList.add(`pawn-image`)
    return pawnElement;
}

function CreateCards(game) {
    let cards = document.getElementsByClassName("Cards");
    for (let i = 0; i < cards.length; i++) {
        cards[i].style.border = "1px solid black";
        cards[i].style.backgroundColor = "White"
    }

    const [player1, player2] = game.players;
    const {moveCards: moveCardsPlayer1} = player1;
    const {moveCards: moveCardsPlayer2} = player2;
    const {extraMoveCard} = game;

    const cardsConfig = [
        {card: moveCardsPlayer1[0], cardId: "Card1", color: moveCardsPlayer1[0].stampColor},
        {card: moveCardsPlayer1[1], cardId: "Card2", color: moveCardsPlayer1[1].stampColor},
        {card: moveCardsPlayer2[0], cardId: "OppesiteCard1", color: moveCardsPlayer2[0].stampColor},
        {card: moveCardsPlayer2[1], cardId: "OppesiteCard2", color: moveCardsPlayer2[1].stampColor},
        {card: extraMoveCard, cardId: "ExtraCard", color: extraMoveCard.stampColor}
    ];

    cardsConfig.forEach(({card, cardId, color}) => CreateTableCard(card, cardId, color));
    if (player1.id === currentUserId) {
        cardsConfig.forEach(element => {
            let cardInArray = false;
            moveCardsPlayer1.forEach(card => {
                if (card === element.card) {
                    cardInArray = true;
                }
            })
            if (cardInArray) {
                const cell = document.getElementById(element.cardId);
                if (idPlayerToPlay === currentUserId) {
                    enableEvents(cell, element, ()=> {selectingCard(element.cardId, element);});

                } else {
                    disableEvents(cell);
                }
            }

        });
    } else if (player2.id === currentUserId) {
        cardsConfig.forEach(element => {
            let cardInArray = false;
            moveCardsPlayer2.forEach(card => {
                if (card === element.card) {
                    cardInArray = true;
                }
            })
            if (cardInArray) {
                const cell = document.getElementById(element.cardId);
                if (idPlayerToPlay === currentUserId) {
                    enableEvents(cell, element, ()=> {selectingCard(element.cardId, element);});
                } else {
                    disableEvents(cell);
                }
            }
        });
    }
}

function ClearBackground() {
    for (let row = 0; row < 5; row++) {
        for (let colmn = 0; colmn < 5; colmn++) {
            let box = document.getElementById(`square-${row}-${colmn}`);
            box.style.backgroundColor = "White";
            box.style.border = "1px solid black";
        }
    }
    playersPlaying.forEach(player => {
        const {row, column} = player.school.templeArchPosition;
        const templeSquareId = `square-${row}-${column}`;
        const templSquare = document.getElementById(templeSquareId);
        templSquare.style.backgroundColor = player.color;
    })
}

function CreateTableCard(Card, cardId, color) {
    if (!Card) return;

    const cardElement = document.getElementById(cardId);
    if (!cardElement) return;

    cardElement.innerHTML = '';
    const imageUrl = `./Assets/Image/MoveCards/${Card.name}.png`;
    const colorUrl = `./Assets/Image/ColorStamps/${color}.png`;

    // Create the main image element
    const imgElement = document.createElement('img');
    imgElement.src = imageUrl;
    imgElement.alt = Card.name;
    imgElement.classList.add('card-image');

    // Create the colorStamp image element
    const colorStamp = document.createElement('img');
    colorStamp.src = colorUrl;
    colorStamp.alt = `${color} stamp`;
    colorStamp.classList.add('color-stamp');

    // Append the images to the card element
    cardElement.appendChild(imgElement);
    cardElement.appendChild(colorStamp);
}

function updateBoardOrientation(playerDirection) {
    let boardContainer = document.getElementById("GameBoard-Container");
    if (playerDirection === 'North') {
        boardContainer.style.transform = 'rotate(180deg)';

        const squares = board.querySelectorAll('.square');
        squares.forEach(square => {
            square.style.transform = 'rotate(180deg)';
        });
        const cards = document.querySelectorAll('.Cards');
        cards.forEach(card => {
            card.style.transform = 'rotate(180deg)';
        })
        const extraCard = document.getElementById('ExtraCard');
        extraCard.style.transform = 'none';
    } else {
        board.style.transform = 'none';
        const squares = board.querySelectorAll('.square');
        squares.forEach(square => {
            square.style.transform = 'none';
        });
    }
}

function selectingPawn(pawn, square) {
    if (idWinner === "00000000-0000-0000-0000-000000000000") {
        ClearBackground();
        if (idPlayerToPlay === currentUserId) {
            if (previousSelectedPawn) {
                previousSelectedPawn.style.border = "1px solid black"
            }
            selectedPawn = pawn;
            square.style.border = "4px solid blue"
            previousSelectedPawn = square;
        }
        if (selectedCard != null && selectedPawn != null) {
            showPossibleMoves();
        }
    }
}

function selectingCard(cardId, card) {
    if (idWinner === "00000000-0000-0000-0000-000000000000") {
        ClearBackground()
        if (idPlayerToPlay === currentUserId) {
            if (previousSelectedCard != null) {
                previousSelectedCard.style.border = "1px solid black"
            }
            cardId = document.getElementById(cardId);
            cardId.style.border = "4px solid blue"
            selectedCard = card.card;
            previousSelectedCard = cardId;
        }
        if (selectedCard != null && selectedPawn != null) {
            showPossibleMoves();
        }
    }
}

async function showPossibleMoves() {
    const url = `https://localhost:5051/api/Games/${gameId}/possible-moves-of/${selectedPawn.Id}/for-card/${selectedCard.name}`;
    const options = {method: "GET", headers: headers};

    try {
        if (idWinner === "00000000-0000-0000-0000-000000000000") {
            const data = await fetchData(url, options);
            if (disableEventSquare === false) {
                for (let row = 0; row < 5; row++) {
                    for (let colmn = 0; colmn < 5; colmn++) {
                        let box = document.getElementById(`square-${row}-${colmn}`);
                        box.style.backgroundColor = "none"
                        box.addEventListener("click", function () {
                            handleMovePawn(row, colmn);
                        });
                    }
                }
                disableEventSquare = true;
            }

            data.forEach(positie => {
                let box = document.getElementById(`square-${positie.to.row}-${positie.to.column}`);
                if (box != null) {
                    box.style.backgroundColor = "lightblue";
                }
            })
        }

    } catch (error) {
        console.error('Error fetching data:', error);
    }
}

async function handleMovePawn(row, column) {
    const toSquare = playMat.grid[row][column];
    if (idWinner === "00000000-0000-0000-0000-000000000000" && (toSquare == null || toSquare.OwnerId !== selectedPawn.OwnerId)) {

        const squareBeforeMoving = `square-${selectedPawn.Position.Row}-${selectedPawn.Position.Column}`
        const url = `https://localhost:5051/api/Games/${gameId}/move-pawn`;
        let myData = {pawnId: selectedPawn.Id, moveCardName: selectedCard.name, to: {row: row, column: column}}
        const options = {
            method: "POST", headers,
            body: JSON.stringify(myData)
        };

        try {
            await fetchData(url, options);
            oldSquare = document.getElementById(squareBeforeMoving);
            disableEvents(oldSquare);
            ClearBackground();
            await SetPawnsOnMat();
            ResetData();
        } catch (error) {
            displayError(error, errorOutput)
        }
    }
}

async function skipMove() {
    const url = `https://localhost:5051/api/Games/${gameId}/skip-movement`;

    let moveCardName;
    if (selectedCard === null) {
        moveCardName = null;

    } else {
        moveCardName = selectedCard.name;
    }


    let myData = {moveCardName: moveCardName}

    const options = {
        method: "POST", headers,
        body: JSON.stringify(myData)
    };

    try {
        await fetchData(url, options);
        ClearBoard()
        await SetPawnsOnMat();
        ResetData();
    } catch (error) {
        displayError(error, errorOutput)
    }
}

function thereIsWinner() {
    if (methodeUsedToWin === null) {
        methodeUsedToWin = "unknown"
    }
    if (idWinner === currentUserId) {
        startingplayer.textContent = "You are the winner!! "
        startingplayer2.textContent = "By using " + methodeUsedToWin + " :D"
        startingplayer.style.color = "Green"
        startingplayer2.style.color = "Green"
    } else {
        startingplayer.textContent = "You lost the game :( "
        startingplayer2.textContent = "By " + methodeUsedToWin
        startingplayer.style.color = "Red"
        startingplayer2.style.color = "Red"

    }
}

function ResetData() {
    selectedPawn = null;
    previousSelectedPawn = null;
    selectedCard = null;
    previousSelectedCard = null;
}

window.addEventListener('beforeunload', async function (e) {
    await LeaveGame();
});

async function possibleMovesOpponent() {
    ClearBackground();
    const url = `https://localhost:5051/api/Games/${gameId}/possible-moves-opponent`;
    const options = {method: "GET", headers: headers};

    try {
        if (idWinner === "00000000-0000-0000-0000-000000000000") {
            const data = await fetchData(url, options);
                for (let row = 0; row < 5; row++) {
                    for (let colmn = 0; colmn < 5; colmn++) {
                        let box = document.getElementById(`square-${row}-${colmn}`);
                        box.style.backgroundColor = "none"
                    }
                }

            data.forEach(positie => {
                let box = document.getElementById(`square-${positie.to.row}-${positie.to.column}`);
                if (box != null) {
                    box.style.backgroundColor = "#FF9999";
                }
            })
        }

    } catch (error) {
        console.error('Error fetching data:', error);
    }
}

setInterval(function () {
    ShowPlayers();
    if (currentUserId !== idOwnerPlayer && firstTime && gameId !== "00000000-0000-0000-0000-000000000000") {
        console.log('start game other')
        intro.textContent = "";
        intro2.textContent = "";
        CreateTable();
        startgameBtn.style.display = "none";
        CurrentPlayerToOrient();

        SetPawnsOnMat();
        gameStarted = true;
        firstTime = false
    }
    if (currentUserId !== idPlayerToPlay) {
        SetPawnsOnMat();
    }
}, 2000);