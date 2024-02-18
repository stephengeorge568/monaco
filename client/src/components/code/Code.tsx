import Editor, { Monaco } from "@monaco-editor/react";
import * as monaco from "monaco-editor/esm/vs/editor/editor.api";
import Connector from "../../utils/SignalrConnection";
import { useCallback, useEffect, useRef, useState } from "react";
import { Operation, fromEvent } from "../../models/Operation";
import { transform } from "src/utils/TransformUtils";
import { Queue } from "src/models/Queue";
//import { transform } from '../../utils/TransformUtils';

export default function Code() {
  const [documentId, setDocumentId] = useState("BigTimeIdOhYeah");
  const webSocket = Connector();
  var isProgrammaticChange = useRef<boolean>(false);
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
        webSocket.newOperation(op, documentId);
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
        }
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
      historyRef.current.get(revisionId.current)?.push(op);

      outgoingOperations.current.enqueue(op);
      sendNextOperation();
    }
  }

  function click() {
    setDocumentId("test");
  }

  function handleEditorDidMount(
    editor: monaco.editor.IStandaloneCodeEditor | null,
    monaco: Monaco
  ) {
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
