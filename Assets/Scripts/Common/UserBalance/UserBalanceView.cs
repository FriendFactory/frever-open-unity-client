using System.Collections;
using Abstract;
using TMPro;
using UnityEngine;

namespace Common.UserBalance
{
    public class UserBalanceView<TUserBalanceModel> : BaseContextDataView<TUserBalanceModel> 
        where TUserBalanceModel: IUserBalanceModel
    {
        [SerializeField] private TMP_Text _softCurrencyText;
        [SerializeField] private TMP_Text _hardCurrencyText;

        private Coroutine _animationRoutine;
        private readonly WaitForEndOfFrame _waitForEndOfFrame = new();

        private void OnDisable()
        {
            if (_animationRoutine != null)
            {
                StopCoroutine(_animationRoutine);
            }
        }

        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            TryAnimateCurrencyValues();

            ContextData.ValuesUpdated += TryAnimateCurrencyValues;
        }

        protected override void BeforeCleanup()
        {
            if (ContextData != null)
            {
                ContextData.ValuesUpdated -= TryAnimateCurrencyValues;
            }

            base.BeforeCleanup();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void TryAnimateCurrencyValues()
        {
            if (gameObject != null && _animationRoutine != null)
            {
                StopCoroutine(_animationRoutine);
            }

            var args = ContextData.Args;

            if (args.FromSoftCurrency == args.ToSoftCurrency &&
                args.FromHardCurrency == args.ToHardCurrency)
            {
                SetCurrencyValues(args.FromSoftCurrency, args.FromHardCurrency);
                return;
            }

            if (gameObject.activeInHierarchy)
            {
                _animationRoutine = StartCoroutine(CurrencyAnimationRoutine());
            }
        }

        private IEnumerator CurrencyAnimationRoutine()
        {
            var args = ContextData.Args;

            SetCurrencyValues(args.FromSoftCurrency, args.FromHardCurrency);

            if (args.StartDelayInSeconds > 0)
            {
                yield return new WaitForSecondsRealtime(args.StartDelayInSeconds);
            }

            var softCurrencyDif = args.ToSoftCurrency - args.FromSoftCurrency;
            var hardCurrencyDif = args.ToHardCurrency - args.FromHardCurrency;

            if (args.AnimationTime > 0)
            {
                var progress = 0.0f;

                while (progress < 1.0f)
                {
                    var currentSoftCurrencyValue = Mathf.FloorToInt(args.FromSoftCurrency + softCurrencyDif * progress);
                    var currentHardCurrencyValue = Mathf.FloorToInt(args.FromHardCurrency + hardCurrencyDif * progress);

                    SetCurrencyValues(currentSoftCurrencyValue, currentHardCurrencyValue);

                    yield return _waitForEndOfFrame;

                    progress += Time.deltaTime / args.AnimationTime;
                }
            }

            SetCurrencyValues(args.ToSoftCurrency, args.ToHardCurrency);
        }
        
        private void SetCurrencyValues(int softCurrency, int hardCurrency)
        {
            _softCurrencyText.text = softCurrency.ToString();
            _hardCurrencyText.text = hardCurrency.ToString();
        }
    }

    public class UserBalanceView : UserBalanceView<IUserBalanceModel> { }
}