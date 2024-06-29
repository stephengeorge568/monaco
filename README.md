# Monaco
Collaborative document editor that allows 1+ users editting the same document simultaneously. Syncing document state across server and clients is done through my implementation of operational transformation. Think Google Docs, but in a code editor.
Proof of concept project.

I initially undertook this project in college in the summer of 2022. I achieved a near perfect solution then, but a few edge case bugs had eluded me and I took a break from the project out of frustration. 2 years later, my implementation of operational transformation is functionally near perfect, if not perfect. While the transformation engine and data model were at its core developed during my formative years in college and were therefore horribly thought through -- it works. Perhaps someday I will come back to rewrite the transformation engine, but that day is not today. Nor tommorrow. Give me another 2 years perhaps :)

If you're interested in undertaking a side project and are a macochist when it comes to headaches, then give operational transformation a try. Close the tab, develop a basic understanding of distributed document syncing solutions, and hit the ground running with your own implementation. *I promise this will be one of the most difficult problems you'll ever undertake independently.*

### What is the problem being solved here
Simultaneous document editing is the goal. This means that you need a process that can sync document state in real time between 2+ clients with zero tolerance for mistakes. If there was no latency between clients, then this would be accomplished automatically, no different than multiple keyboards attached to the same computer editting the same local document. However, latency causes the intention and natural causality of document changes to be lost. In other terms, two conccurent operations can be executed in different orders on two different copies of the same document -- which compounds with more concurrent operations.

## How to run...

### Frontend

Update [SignalrConnection.ts](https://github.com/stephengeorge568/monaco/blob/main/client/src/utils/SignalrConnection.ts#L3) with the IP address of the device on your network running the backend.

`cd client && npm start`

Access webpage using any browser at YOUR_BACKEND_IP:3000

### Backend

Install .NET core 8

`cd server/src && dotnet build && dotnet run`

Alternatively, vscode is configured to be able to launch the backend in debugging mode.

## Tech details
- React (CRA), Typescript, Jest, SignalR, Monaco Code Editor library
- dotnet, SignalR, xunit

## TODO
- Write up documentation on how I am resolving document state conflicts.
- Refactor transformer
- Refactor data model to be startIndex, endIndex. Instead of column and line based ??
- Investigate if resolving conflicting ranges requires a possible introduction of secondary operation