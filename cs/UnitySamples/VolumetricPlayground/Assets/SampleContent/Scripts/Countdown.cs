using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Countdown : MonoBehaviour
{
    [SerializeField]
    private GameObject[] numbers;


    [SerializeField]
    UnityEvent _countDownComplete = new UnityEvent();

    void Start()
    {
        enabled = false; // Disable the script to prevent it from running immediately
    }

    private void OnEnable()
    {
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i].transform.localScale = Vector3.one * .001f;
        }
        StartCoroutine(CountdownCoroutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i].transform.localScale = Vector3.one * .001f;
        }
    }

    private IEnumerator CountdownCoroutine()
    {
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i].transform.localScale = Vector3.one;
            yield return new WaitForSeconds(1f);
            numbers[i].transform.localScale = Vector3.one * .001f;
        }

        _countDownComplete?.Invoke();
        enabled = false;
    }
}
