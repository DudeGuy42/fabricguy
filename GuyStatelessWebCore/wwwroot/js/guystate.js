"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/guyhub").build();

//Disable send button until connection is established
// document.getElementById("sendButton").disabled = true;

connection.on("broadcastMessage", function (guyName, guyMessage) {
    console.log(guyName + ": " + guyMessage);
});

connection.on("receiveInfo", function (message) {
    console.log(message);
});

connection.on("connected", function (message) {
    console.log(message);
});

connection.on("disconnected", function (message) {
    console.log(message);
});

connection.start().then(function () {
    console.log("Started Connection to guyhub.");
    connection.stream("Counter")
        .subscribe({
            next: (item) => {
                console.log(item);
            },
            complete: () => {
                console.log("Stream complete.");
            },
            error: (err) => {
                console.error(err);
            }
        });
    //document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

//document.getElementById("sendButton").addEventListener("click", function (event) {
//    var user = document.getElementById("userInput").value;
//    var message = document.getElementById("messageInput").value;
//    connection.invoke("SendMessage", user, message).catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});