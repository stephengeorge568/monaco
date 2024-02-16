import Editor, { Monaco } from '@monaco-editor/react';
import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';
import { onOperationTransformedAck } from '../../services/ot.service'
import Connector from '../../utils/SignalrConnection';
import { useCallback, useEffect, useRef, useState } from 'react';
import { OpHistory, Operation, fromEvent } from '../../models/Operation';
//import { transform } from '../../utils/TransformUtils';

export default function Code() {
  const [documentId, setDocumentId] = useState("BigTimeIdOhYeah");
  const webSocket = Connector();
  var isProgrammaticChange = useRef<boolean>(false);
  // ive seen someone use state for this, look into
  const editorRef = useRef<monaco.editor.IStandaloneCodeEditor | null>(null);
  const historyRef = useRef<Map<number, Operation[]>>(new Map());

  const onOperationReceived = useCallback((op: Operation) => {
    if (op.originatorId !== webSocket.connection.connectionId) {
      isProgrammaticChange.current = true;
      editorRef.current?.executeEdits('server',
        [
          {
          range: {
            startLineNumber: op.startLine,
            endLineNumber: op.endLine,
            startColumn: op.startColumn,
            endColumn: op.endColumn,
          },
          text: op.text,
          },
        ],
      // endCursorState TODO
      );
    isProgrammaticChange.current = false;
    }
  }, [webSocket]);

  useEffect(() => {
    webSocket.events(onOperationReceived, onOperationTransformedAck);
  }, [webSocket, onOperationReceived]);

  function onModelChange(value: string | undefined, event: monaco.editor.IModelContentChangedEvent) {
    console.log(
      event.changes[0].text,
      event.changes[0].text.length,
      event.changes[0].text.split(new RegExp('\r\n|\r|\n')).length - 1);
    if (!isProgrammaticChange.current) {
      let op: Operation = fromEvent(event, webSocket.connection.connectionId ?? '');
      //op = transform(op, historyRef.current);
      webSocket.newOperation(op, documentId);
    }
  }

  function click() {
    setDocumentId('test');
  }

  function handleEditorDidMount(editor: monaco.editor.IStandaloneCodeEditor | null, monaco: Monaco) {
    webSocket.addToGroup(documentId);
    editorRef.current = editor;
  }

  return (
    // https://www.npmjs.com/package/@monaco-editor/react
    <>
      <button onClick={click}>
        Press
      </button>
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
