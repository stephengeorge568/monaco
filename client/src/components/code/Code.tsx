import Editor, { Monaco } from "@monaco-editor/react";
import * as monaco from "monaco-editor/esm/vs/editor/editor.api";
import Connector from "../../utils/SignalrConnection";
import { useCallback, useEffect, useRef, useState } from "react";
import { Operation, copy, fromEvent } from "../../models/Operation";
import { transform } from "src/utils/TransformUtils";
import { Queue } from "src/models/Queue";

export default function Code() {
  const [documentId] = useState("BigTimeIdOhYeah");
  const webSocket = Connector();
  const isProgrammaticChange = useRef<boolean>(false);
  // ive seen someone use state for this, look into
  const editorRef = useRef<monaco.editor.IStandaloneCodeEditor | null>(null);
  const historyRef = useRef<Map<number, Operation[]>>(new Map());
  const revisionId = useRef<number>(0);
  const outgoingOperations = useRef<Queue<Operation>>(new Queue());
  const isSendingOperation = useRef<boolean>(false);

  const sendNextOperation = useCallback(() => {
    if (!isSendingOperation.current) {
      let op: Operation | undefined = outgoingOperations.current.dequeue();
      if (op) {
        isSendingOperation.current = true;
        console.log('Outgoing Pre', copy(op));
        let transformedOp = transform(op, historyRef.current);
        console.log('Outgoing Trans', copy(transformedOp[0]));
        transformedOp[0].revisionId = revisionId.current;
        // TODO stop this 2 operations bullshit from college
        webSocket.newOperation(transformedOp[0], documentId);
      }
    }
  }, [documentId, webSocket]);

  const onOperationTransformedAck = useCallback(
    (newRevisionId: number) => {
      console.log("New revisionId from ACK: ", newRevisionId);
      revisionId.current = newRevisionId;
      isSendingOperation.current = false;
      sendNextOperation();
    },
    [sendNextOperation]
  );

  const onOperationReceived = useCallback(
    (op: Operation) => {
      if (op.originatorId !== webSocket.connection.connectionId) {
        console.log('Recieved pre: ', copy(op));
        let transformedOps: Operation[] = transform(op, historyRef.current);
        console.log('Recieved trans: ', copy(transformedOps[0]));
        isProgrammaticChange.current = true;
        for (var change of transformedOps) {
          editorRef.current?.executeEdits(
            "server",
            [
              {
                range: {
                  startLineNumber: change.startLine,
                  endLineNumber: change.endLine,
                  startColumn: change.startColumn,
                  endColumn: change.endColumn,
                },
                text: change.text,
              },
            ]
            // endCursorState TODO
          );

          // might not need this if check, do always potentially
          if (revisionId.current < op.revisionId) {
            revisionId.current = op.revisionId;
          }

          let newHistory: Map<number, Operation[]> = historyRef.current;
          if (!newHistory.has(op.revisionId)) {
            newHistory.set(op.revisionId, []);
          }
          newHistory.get(op.revisionId)?.push(op);
          historyRef.current = newHistory;
        }
        isProgrammaticChange.current = false;
        console.log('New rev Id from propogation: ', op.revisionId);
      }
    },
    [webSocket.connection.connectionId]
  );

  useEffect(() => {
    webSocket.events(onOperationReceived, onOperationTransformedAck);
  }, [
    webSocket,
    sendNextOperation,
    onOperationReceived,
    onOperationTransformedAck,
  ]);

  function onModelChange(
    value: string | undefined,
    event: monaco.editor.IModelContentChangedEvent
  ) {
    if (!isProgrammaticChange.current) {
      let op: Operation = fromEvent(
        event,
        webSocket.connection.connectionId ?? "",
        revisionId.current
      );

      let newHistory: Map<number, Operation[]> = historyRef.current;

      if (!newHistory.has(op.revisionId)) {
        newHistory.set(op.revisionId, []);
      }
      newHistory.get(op.revisionId)?.push(copy(op));
      historyRef.current = newHistory;
      outgoingOperations.current.enqueue(op);
      sendNextOperation();
    }
  }

  function click() {
    console.log(historyRef.current);
    console.log(outgoingOperations.current);
  }

  function handleEditorDidMount(
    editor: monaco.editor.IStandaloneCodeEditor | null,
    monaco: Monaco
  ) {
    editor?.getModel()?.setEOL(monaco.editor.EndOfLineSequence.LF);
    editor?.updateOptions({ quickSuggestions: false });
    webSocket.addToGroup(documentId);
    editorRef.current = editor;
  }

  return (
    // https://www.npmjs.com/package/@monaco-editor/react
    <>
      <button onClick={click}>Press</button>
      <Editor
        height="100vh"
        defaultLanguage="typescript"
        onChange={onModelChange}
        onMount={handleEditorDidMount}
        theme="vs-dark"
      />
    </>
  );
}
