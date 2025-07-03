using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.classes.utils.fonts
{
    public class SimplePacker
    {
        private readonly int _maxWidth;
        private int _currentY;
        private int _currentX;
        private int _currentLineHeight;

        public int Height { get; private set; }

        public SimplePacker(int maxWidth) => _maxWidth = maxWidth;

        public void Pack(IEnumerable<Glyph> glyphs)
        {
            _currentY = 0;
            _currentX = 0;
            _currentLineHeight = 0;

            foreach (var glyph in glyphs.OrderByDescending(g => g.Size.Height))
            {
                if (_currentX + glyph.Size.Width > _maxWidth)
                {
                    _currentY += _currentLineHeight;
                    _currentX = 0;
                    _currentLineHeight = 0;
                }

                glyph.X = _currentX;
                glyph.Y = _currentY;

                _currentX += glyph.Size.Width + 1; // +1px минимальный отступ
                _currentLineHeight = Math.Max(_currentLineHeight, glyph.Size.Height);
                Height = Math.Max(Height, _currentY + _currentLineHeight);
            }
        }
    }
}
