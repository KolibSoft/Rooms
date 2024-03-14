# Kolib Software - Rooms #

- [Room Protocol](#room-protocol)
- [Room Hub Routing](#room-hub-routing)
- [Room Channel Conversion](#room-channel-conversion)
- [Javascript Utils File](./Rooms.Web/wwwroot/rooms.js)
- [Room Test Server](https://krooms.azurewebsites.net/)

## Room ##

A **Room** is a connection point where participants can send and receive messages. Once connected, a participant has the right to remain in the **Room** and cannot be removed, however the rest of the participants can agree to ignore them. Support TCP and Web Socket based connections.

## Room Protocol #

**Room** is a Websocket subprotocol designed to easily share messages between multiple sockets connected through a central point using UTF8 text:

```txt
// Format:
<ROOM-VERB> <ROOM-CHANNEL>
<ROOM-CONTENT>

// Example:
MSG 12345678
UTF8 Text Content
```

- **Room Verb** is a sequence of 3 uppercase or lowercase letters of the ASCII code. The **Room Verb** does not have any special meaning for the server that relays the messages, it is the sockets that receive the message that are in charge of interpreting it and how to act in response.

- **Room Channel** is an 8-digit hexadecimal number that represents a 32-bit unsigned integer. The **Room Channel** represents the specific connection point of two sockets, it is the server that is responsible for providing the same channel identifier for both parties, this is achieved by performing an XOR operation between the socket identifiers.

- **Room Content** is a variable length UTF8 text.

## Room Hub Routing ##

When a message is sent to the relay server with channel identifier `0000000` (Loopback) the server must echo the message back to its author.

When a message is sent to the relay server with the channel identifier `ffffffff` (Broadcast), the server must broadcast the message to all participants except the author. The channel of the transmitted message in this case must be converted, this is achieved by performing an XOR operation between the sender and the receiver of the message.

When a message is sent to the relay server with any identifier other than `00000000` or `ffffffff` the server must identify the recipient of the message referenced by the channel to relay it. The receiver's channel identifier can be obtained by performing an XOR operation between the sender's identifier and the channel identifier present in the message.

## Room Channel Conversion ##

In case you want to refer to a specific channel within the body of a message, a conversion of that channel's identifier must be performed, this is achieved by performing an XOR operation between the identifier of the channel that sends the message and the identifier of the channel it refers to within the content of the message.
