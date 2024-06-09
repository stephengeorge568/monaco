import Editor, { Monaco } from "@monaco-editor/react";
import * as monaco from "monaco-editor/esm/vs/editor/editor.api";
import Connector from "../../utils/SignalrConnection";
import { useCallback, useEffect, useRef, useState } from "react";
import { Operation, fromEvent } from "../../models/Operation";
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
      isSendingOperation.current = true;
      let op: Operation | undefined = outgoingOperations.current.dequeue();
      if (op) {
        let transformedOp = transform(op, historyRef.current);
        // TODO stop this 2 operations bullshit from college
        webSocket.newOperation(transformedOp[0], documentId);
      }
    }
  }, [documentId, webSocket]);

  const onOperationTransformedAck = useCallback(
    (newRevisionId: number) => {
      revisionId.current = newRevisionId;
      isSendingOperation.current = false;
      sendNextOperation();
    },
    [sendNextOperation]
  );

  const onOperationReceived = useCallback(
    (op: Operation) => {
      if (op.originatorId !== webSocket.connection.connectionId) {
        let transformedOps: Operation[] = transform(op, historyRef.current);
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
          let newHistory: Map<number, Operation[]> = historyRef.current;
          if (!newHistory.has(op.revisionId)) {
            newHistory.set(op.revisionId, []);
          }
          newHistory.get(op.revisionId)?.push(op);
          historyRef.current = newHistory;
        }

        revisionId.current = op.revisionId;
        
        isProgrammaticChange.current = false;
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

      if (op.text !== 'a')
      {
        // console.log('shit broke champ');
      }

      let newHistory: Map<number, Operation[]> = historyRef.current;

      if (!newHistory.has(revisionId.current)) {
        newHistory.set(revisionId.current, []);
      }
      newHistory.get(revisionId.current)?.push(op);
      historyRef.current = newHistory;
      outgoingOperations.current.enqueue(op);
      sendNextOperation();
    }
  }

  function click() {
    historyRef.current = new Map();
    revisionId.current = 0;
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
