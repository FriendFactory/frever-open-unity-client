using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Navigation.Core;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace Navigation.Args
{
    public sealed class CharacterStyleSelectionArgs : PageArgs, IStyleSelectionArgs, IGenderSelectionArgs, ICreateFreverArgs
    {
        public override PageId TargetPage => PageId.CharacterStyleSelection;

        public long? SelectedStyle { get; set; }
       
        public Action<CharacterInfo, Gender> OnStyleSelected { get; set; }
        public Action<Gender> OnSelfieButtonClicked { get; set; }
        public Action<CharacterInfo, Gender> OnBackButtonClicked { get; set; }
        public Action<Gender> GenderSelectedAction { get; set; }
        public Action OnPageDispayed { get; set; }
        public bool ShowBackButton { get; set; }
        public Gender SelectedGender { get; set; }
        public Race Race { get; set; }
        public Action SelfieModeSelected { get; set; }
        public Action PresetModeSelected { get; set; }
        public Action BackButtonClick { get; set; }
        public CreateMode? SelectedCreateMode { get; set; }
    }
}