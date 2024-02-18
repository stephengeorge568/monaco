import * as signalR from "@microsoft/signalr";
import { Operation } from "../models/Operation";
const URL = process.env.HUB_ADDRESS ?? "http://localhost:5070/opHub";
class Connector {
  public connection: signalR.HubConnection;
  public events: (
    onOperationReceived: (operation: Operation) => void,
    onOperationTransformedAck: (newRevisionId: number) => void
  ) => void;
  static instance: Connector;
  private hasInitEvents: boolean = false;

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(URL)
      .withAutomaticReconnect()
      .build();
    this.connection.start().catch((err) => console.log(err));
    this.events = (onOperationReceived, onOperationTransformedAck) => {
      if (!this.hasInitEvents) {
        this.connection.on("operationRecieved", (operation) => {
          onOperationReceived(operation);
        });
        this.connection.on("operationTransformedAck", (newRevisionId) => {
          onOperationTransformedAck(newRevisionId);
        });
        this.hasInitEvents = true;
      }
    };
  }
  public newOperation = (op: Operation, documentId: string) => {
    this.connection
      .send("newOperation", op, documentId)
      .then((x) => console.log("newOperation sent"));
  };
  public addToGroup = (groupName: string) => {
    this.connection
      .send("addToGroup", groupName)
      .then((x) => console.log("AddToGroup sent"));
  };
  public static getInstance(): Connector {
    if (!Connector.instance) Connector.instance = new Connector();
    return Connector.instance;
  }
}
export default Connector.getInstance;
