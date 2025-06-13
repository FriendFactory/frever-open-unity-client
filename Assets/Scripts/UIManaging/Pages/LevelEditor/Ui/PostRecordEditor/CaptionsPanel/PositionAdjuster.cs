using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class PositionAdjuster: MonoBehaviour
    {
        [SerializeField] private AdjustPositionLine[] _lines;
        [SerializeField] private DeleteCaptionArea _deleteCaptionArea;
        
        private Vector2 _touchPosition;
        private Vector2 _rawPosition;

        private readonly Dictionary<LineAxis, bool> _stickingStateAxisTable = new Dictionary<LineAxis, bool>
        {
            { LineAxis.Horizontal, false },
            { LineAxis.Vertical, false }
        };
        
        //---------------------------------------------------------------------
        // Events 
        //---------------------------------------------------------------------

        public event Action RulersEnabled;
        public event Action RulersDisabled;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(RectTransform viewPort)
        {
            SetupHorizontalLinePosition(viewPort);

            foreach (var line in _lines)
            {
                line.HideImmediate();
            }
        }

        public Vector2 GetAdjustedPosition(Vector2 rawPosition)
        {
            _rawPosition = rawPosition;
            var adjustedPos = rawPosition;
            if (ShouldStick(LineAxis.Horizontal, rawPosition))
            {
                adjustedPos.y = GetLinePosition(LineAxis.Horizontal).y;
            }
            
            if (ShouldStick(LineAxis.Vertical, rawPosition))
            {
                adjustedPos.x = GetLinePosition(LineAxis.Vertical).x;
            }

            return adjustedPos;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void LateUpdate()
        {
            foreach (var line in _lines)
            {
                UpdateLineState(line);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private bool ShouldStick(LineAxis lineAxis, Vector2 rawPosition)
        {
            var shouldStick = _lines.First(x => x.Axis == lineAxis).ShouldStick(rawPosition);

            if (shouldStick == _stickingStateAxisTable[lineAxis]) return shouldStick;
            
            _stickingStateAxisTable[lineAxis] = shouldStick;
            
            if (shouldStick)
            {
                RulersEnabled?.Invoke();
            }
            else
            {
                RulersDisabled?.Invoke();
            }

            return shouldStick;
        }

        private Vector3 GetLinePosition(LineAxis lineAxis)
        {
            return _lines.First(x => x.Axis == lineAxis).transform.position;
        }
        
        private void UpdateLineState(AdjustPositionLine line)
        {
            if (_deleteCaptionArea.IsCaptionInsideArea)
            {
                line.Hide();
                return;
            }
            
            switch (line.ShouldStick(_rawPosition))
            {
                case true when !line.IsShown:
                    line.Show();
                    break;
                case false when line.IsShown:
                    line.Hide();
                    break;
            }
        }
        
        private void SetupHorizontalLinePosition(RectTransform viewPort)
        {
            var horizontalLine = _lines.First(x => x.Axis == LineAxis.Horizontal);
            var horizLinePos = horizontalLine.transform.position;
            var viewPortCenter = viewPort.GetWorldCenterPosition();
            horizLinePos.y = viewPortCenter.y;
            horizontalLine.transform.position = horizLinePos;
        }
    }
}