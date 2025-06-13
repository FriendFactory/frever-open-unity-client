using System.Collections.Generic;

namespace Common
{
    public sealed class UndoRedoHelper
    {
        public bool UndoNotEmpty => _undoStack.Count > 0;
        public bool RedoNotEmpty => _redoStack.Count > 0;

        private readonly Stack<UserCommand> _undoStack = new Stack<UserCommand>();
        private readonly Stack<UserCommand> _redoStack = new Stack<UserCommand>();

        public void RegisterCommand(UserCommand command)
        {
            _undoStack.Push(command);
            DisposeRedoStack();
        }

        public void Undo()
        {
            if (_undoStack.Count == 0) return;

            var command = _undoStack.Pop();
            command.CancelCommand();
            _redoStack.Push(command);
        }

        public void Redo()
        {
            if (_redoStack.Count == 0) return;

            var command = _redoStack.Pop();
            command.ExecuteCommand();
            _undoStack.Push(command);
        }

        public void Clear()
        {
            DisposeUndoStack();
            DisposeRedoStack();
        }

        private void DisposeUndoStack()
        {
            while (_undoStack.Count > 0)
            {
                var command = _undoStack.Pop();
                command.Dispose();
            }
        }

        private void DisposeRedoStack()
        {
            while (_redoStack.Count > 0)
            {
                var command = _redoStack.Pop();
                command.Dispose();
            }
        }
    }
}