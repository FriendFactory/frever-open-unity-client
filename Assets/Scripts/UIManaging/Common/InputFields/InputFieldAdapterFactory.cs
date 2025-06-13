using System;
using JetBrains.Annotations;
using TMPro;
using UIManaging.SnackBarSystem;
using Zenject;

#if ADVANCEDINPUTFIELD_TEXTMESHPRO
using AdvancedInputFieldPlugin;
#endif

namespace UIManaging.Common.InputFields
{
    [UsedImplicitly]
    public sealed class InputFieldAdapterFactory
    {
        [Inject] private SnackBarHelper _snackBarHelper;

        public IInputFieldAdapter CreateInstance(object inputField, bool richTextSupport = false)
        {
            switch (inputField)
            {
#if ADVANCEDINPUTFIELD_TEXTMESHPRO
                case AdvancedInputField field:
                {
                    var adapter = richTextSupport
                        ? new AdvancedInputFieldRichTextAdapter(field) as IInputFieldAdapter
                        : new AdvancedInputFieldAdapter(field);
                    return adapter;
                }
#endif
                case TMP_InputField field:
                    return new TMPInputFieldAdapter(field, _snackBarHelper);
            }

            throw new NotImplementedException($"Adapter for the {inputField.GetType().Name} has not found");
        }
    }
}