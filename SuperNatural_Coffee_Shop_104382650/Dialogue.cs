using System.Collections.Generic;
using System; // For Console.WriteLine

namespace CoffeeShop
{
    /// <summary>
    /// Represents a sequence of dialogue lines, each associated with a speaker.
    /// Manages the progression through the dialogue.
    /// </summary>
    public class Dialogue
    {
        /// <summary>
        /// The list of speakers for each dialogue line.
        /// </summary>
        private List<string> _speakers;
        /// <summary>
        /// The list of text content for each dialogue line.
        /// </summary>
        private List<string> _texts;
        /// <summary>
        /// The index of the currently active dialogue line. -1 indicates the dialogue hasn't started or is reset.
        /// </summary>
        private int _currentLineIndex;

        /// <summary>
        /// Gets a read-only list of the speakers in the dialogue.
        /// </summary>
        public IReadOnlyList<string> Speakers
        {
            get { return _speakers.AsReadOnly(); }
        }

        /// <summary>
        /// Gets a read-only list of the text lines in the dialogue.
        /// </summary>
        public IReadOnlyList<string> Texts
        {
            get { return _texts.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the index of the current dialogue line.
        /// -1 typically means the dialogue is at the beginning or has been reset.
        /// </summary>
        public int CurrentLineIndex
        {
            get { return _currentLineIndex; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialogue"/> class with no lines.
        /// </summary>
        public Dialogue()
        {
            _speakers = new List<string>();
            _texts = new List<string>();
            _currentLineIndex = -1; // Dialogue starts before the first line
        }

        /// <summary>
        /// Adds a new line of dialogue with its associated speaker.
        /// </summary>
        /// <param name="speaker">The name of the character speaking the line. Should not be null.</param>
        /// <param name="text">The content of the dialogue line. Should not be null.</param>
        public void AddLine(string speaker, string text)
        {
            if (speaker == null || text == null)
            {
                Console.WriteLine("Error: Speaker and text should not be null when adding a dialogue line.");
                return;
            }
            _speakers.Add(speaker);
            _texts.Add(text);
        }

        /// <summary>
        /// Advances the dialogue to the next line.
        /// </summary>
        /// <returns><c>true</c> if successfully advanced to a new line; <c>false</c> if there are no more lines or the dialogue is empty.</returns>
        public bool AdvanceToNextLine()
        {
            // Check if lists are null or empty, or if already at/past the last line.
            if (_texts == null || _texts.Count == 0 || _currentLineIndex >= _texts.Count - 1)
            {
                _currentLineIndex = _texts?.Count ?? 0;
                return false;
            }
            _currentLineIndex++;
            return true;
        }

        /// <summary>
        /// Gets the speaker of the current dialogue line.
        /// </summary>
        /// <returns>The name of the current speaker, or <c>null</c> if the current index is out of bounds or dialogue is empty.</returns>
        public string? GetCurrentSpeaker()
        {
            if (_currentLineIndex >= 0 && _currentLineIndex < _speakers.Count)
            {
                return _speakers[_currentLineIndex];
            }
            return null;
        }

        /// <summary>
        /// Gets the text of the current dialogue line.
        /// </summary>
        /// <returns>The content of the current dialogue line, or <c>null</c> if the current index is out of bounds or dialogue is empty.</returns>
        public string? GetCurrentText()
        {
            if (_currentLineIndex >= 0 && _currentLineIndex < _texts.Count)
            {
                return _texts[_currentLineIndex];
            }
            return null;
        }

        /// <summary>
        /// Resets the dialogue progression to the beginning (before the first line).
        /// </summary>
        public void Reset()
        {
            _currentLineIndex = -1;
        }

        /// <summary>
        /// Checks if there are more lines available in the dialogue after the current one.
        /// </summary>
        /// <returns><c>true</c> if there is at least one more line to advance to; otherwise, <c>false</c>.</returns>
        public bool HasMoreLines()
        {
            return _texts != null && _currentLineIndex < (_texts.Count - 1);
        }

        /// <summary>
        /// Clears all dialogue lines and speakers, and resets the current line index.
        /// </summary>
        public void ClearLines()
        {
            _speakers.Clear();
            _texts.Clear();
            _currentLineIndex = -1;
        }
    }
}