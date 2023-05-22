using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class SlideTween : MonoBehaviour
{
    [SerializeField] private RectTransform chasis;
    [SerializeField] private RectTransform component;
    [SerializeField] private RectTransform grip;

    private Vector2 endpoint = new Vector2(20, 20);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void slideTest()
    {
        chasis.DOAnchorPos(endpoint, 2f);
    }
}
