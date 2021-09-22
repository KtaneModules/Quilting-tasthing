using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using rnd = UnityEngine.Random;

public class quilting : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;

    public KMSelectable[] buttons;
    public Color[] buttonColors;

    private int puzzleIndex;
    private List<patch> patches = new List<patch>();
    private patch whitePatch;
    private List<patch> displayedPatches = new List<patch>();
    private List<patch> givenPatches = new List<patch>();
    private QColor[] buttonOrder = new QColor[4];
    private QColor solution;

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;

    private void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable button in buttons)
            button.OnInteract += delegate () { PressButton(button); return false; };
    }

    private void Start()
    {
        puzzleIndex = rnd.Range(0, 5);
        buttonOrder = Enumerable.Range(0, 4).Select(x => (QColor)x).ToList().Shuffle().ToArray();
        for (int i = 0; i < 4; i++)
            buttons[i].GetComponent<Renderer>().material.color = buttonColors[(int)buttonOrder[i]];
        Debug.LogFormat("[Quilting #{0}] Using puzzle {1}.", moduleId, puzzleIndex + 1);
        Debug.LogFormat("[Quilting #{0}] Button colors: {1}", moduleId, buttonOrder.Join(", "));

        GetPatches();
        var attempts = 1;
    tryAgain:
        try
        {
            var amounts = new int[] { 5, 5, 5, 5 };
            for (int i = 0; i < 20; i++)
            {
                patches[i].color = Enumerable.Range(0, 4).Select(x => (QColor)x).Where(x => !patches[i].connections.Any(xx => xx.color == x) && amounts[(int)x] != 0).PickRandom();
                amounts[(int)patches[i].color]--;
            }
            Debug.LogFormat("<Quilting #{0}> Generated a map in {1} attempts.", moduleId, attempts);
        }
        catch (InvalidOperationException)
        {
            attempts++;
            goto tryAgain;
        }
        whitePatch = patches.PickRandom();
        givenPatches = patches.Where(x => x.id != whitePatch.id).ToList();
        // REDUCE
        foreach (QColor[] foundSolution in Recurse(Enumerable.Repeat(QColor.notSet, 20).ToArray()))
        {
            Debug.Log(foundSolution[whitePatch.id].ToString());
        }
        displayedPatches = givenPatches.ToList();
        displayedPatches.Add(whitePatch);
        displayedPatches.Shuffle();
        solution = whitePatch.color;
        Debug.LogFormat("[Quilting #{0}] The color of the white patch is {1}.", moduleId, solution);
        // DISPLAY PATCHES COROUTINE
        if (Application.isEditor)
        {
            Debug.LogFormat("ID {0}", whitePatch.id);
            for (int i = 0; i < 20; i++)
                Debug.LogFormat("Patch {0}: {1}", patches[i].id, patches[i].color);
        }
    }

    private void PressButton(KMSelectable button)
    {
        if (moduleSolved)
            return;
        var ix = Array.IndexOf(buttons, button);
        Debug.LogFormat("[Quilting #{0}] You pressed the {1} button.", moduleId, buttonOrder[ix]);
        if (buttonOrder[ix] == solution)
        {
            module.HandlePass();
            moduleSolved = true;
            Debug.LogFormat("[Quiltng #{0}] That was correct. Module solved!", moduleId);
        }
        else
        {
            module.HandleStrike();
            Debug.LogFormat("[Quiltng #{0}] That was incorrect. Strike!", moduleId);
        }
    }

    private void GetPatches()
    {
        for (int i = 0; i < 20; i++)
            patches.Add(new patch(i, QColor.notSet, null));

        switch (puzzleIndex)
        {
            case 0:
                patches[0].connections = new List<patch> { patches[1] };
                patches[1].connections = new List<patch> { patches[0], patches[2], patches[6], patches[7] };
                patches[2].connections = new List<patch> { patches[1], patches[8] };
                patches[3].connections = new List<patch> { patches[8], patches[4] };
                patches[4].connections = new List<patch> { patches[3], patches[5], patches[9] };
                patches[5].connections = new List<patch> { patches[4], patches[10] };
                patches[6].connections = new List<patch> { patches[1], patches[14] };
                patches[7].connections = new List<patch> { patches[1], patches[8], patches[14] };
                patches[8].connections = new List<patch> { patches[2], patches[3], patches[7], patches[9], patches[14] };
                patches[9].connections = new List<patch> { patches[4], patches[8], patches[10], patches[12], patches[17], patches[18] };
                patches[10].connections = new List<patch> { patches[5], patches[9], patches[11] };
                patches[11].connections = new List<patch> { patches[10], patches[12] };
                patches[12].connections = new List<patch> { patches[9], patches[11], patches[19] };
                patches[13].connections = new List<patch> { patches[14], patches[15] };
                patches[14].connections = new List<patch> { patches[7], patches[8], patches[13], patches[16], patches[17] };
                patches[15].connections = new List<patch> { patches[13], patches[16] };
                patches[16].connections = new List<patch> { patches[14], patches[15], patches[17] };
                patches[17].connections = new List<patch> { patches[9], patches[14], patches[16], patches[18] };
                patches[18].connections = new List<patch> { patches[9], patches[17], patches[19] };
                patches[19].connections = new List<patch> { patches[12], patches[18] };
                break;
            case 1:
                patches[0].connections = new List<patch> { patches[1] };
                patches[1].connections = new List<patch> { patches[0], patches[2], patches[3] };
                patches[2].connections = new List<patch> { patches[1], patches[3], patches[5], patches[6], patches[8] };
                patches[3].connections = new List<patch> { patches[1], patches[2], patches[4], patches[13] };
                patches[4].connections = new List<patch> { patches[3], patches[17] };
                patches[5].connections = new List<patch> { patches[2], patches[7] };
                patches[6].connections = new List<patch> { patches[2], patches[7], patches[10] };
                patches[7].connections = new List<patch> { patches[5], patches[6], patches[8], patches[9] };
                patches[8].connections = new List<patch> { patches[2], patches[7], patches[10], patches[14] };
                patches[9].connections = new List<patch> { patches[7], patches[10] };
                patches[10].connections = new List<patch> { patches[6], patches[8], patches[9], patches[11], patches[12] };
                patches[11].connections = new List<patch> { patches[10] };
                patches[12].connections = new List<patch> { patches[10], patches[14], patches[18] };
                patches[13].connections = new List<patch> { patches[3], patches[14], patches[15], patches[17] };
                patches[14].connections = new List<patch> { patches[8], patches[12], patches[13], patches[15], patches[18] };
                patches[15].connections = new List<patch> { patches[13], patches[14], patches[16], patches[17], patches[18] };
                patches[16].connections = new List<patch> { patches[15] };
                patches[17].connections = new List<patch> { patches[4], patches[13], patches[15], patches[18], patches[19] };
                patches[18].connections = new List<patch> { patches[12], patches[14], patches[15], patches[17], patches[19] };
                patches[19].connections = new List<patch> { patches[17], patches[18] };
                break;
            case 2:
                patches[0].connections = new List<patch> { patches[1], patches[2], patches[5] };
                patches[1].connections = new List<patch> { patches[0], patches[5] };
                patches[2].connections = new List<patch> { patches[0], patches[3], patches[5], patches[6], patches[10] };
                patches[3].connections = new List<patch> { patches[2], patches[4] };
                patches[4].connections = new List<patch> { patches[3], patches[5] };
                patches[5].connections = new List<patch> { patches[0], patches[1], patches[2], patches[4], patches[6], patches[8] };
                patches[6].connections = new List<patch> { patches[2], patches[5], patches[7], patches[8], patches[10] };
                patches[7].connections = new List<patch> { patches[6], patches[8], patches[11] };
                patches[8].connections = new List<patch> { patches[5], patches[6], patches[7], patches[9], patches[12], patches[15], patches[19] };
                patches[9].connections = new List<patch> { patches[8], patches[14], patches[18] };
                patches[10].connections = new List<patch> { patches[2], patches[6], patches[11], patches[12] };
                patches[11].connections = new List<patch> { patches[7], patches[10], patches[12] };
                patches[12].connections = new List<patch> { patches[8], patches[11], patches[13], patches[15], patches[17] };
                patches[13].connections = new List<patch> { patches[12], patches[15], patches[16] };
                patches[14].connections = new List<patch> { patches[9], patches[15], patches[18] };
                patches[15].connections = new List<patch> { patches[8], patches[12], patches[13], patches[14], patches[16], patches[17] };
                patches[16].connections = new List<patch> { patches[13], patches[15] };
                patches[17].connections = new List<patch> { patches[12], patches[15], patches[19] };
                patches[18].connections = new List<patch> { patches[9], patches[14], patches[19] };
                patches[19].connections = new List<patch> { patches[8], patches[15], patches[17], patches[18] };
                break;
            case 3:
                patches[0].connections = new List<patch> { patches[1], patches[5] };
                patches[1].connections = new List<patch> { patches[0], patches[2], patches[10] };
                patches[2].connections = new List<patch> { patches[1], patches[3] };
                patches[3].connections = new List<patch> { patches[2], patches[4], patches[10] };
                patches[4].connections = new List<patch> { patches[3], patches[10] };
                patches[5].connections = new List<patch> { patches[0], patches[6], patches[11] };
                patches[6].connections = new List<patch> { patches[5], patches[7], patches[12] };
                patches[7].connections = new List<patch> { patches[6], patches[8], patches[10], patches[13] };
                patches[8].connections = new List<patch> { patches[7], patches[9], patches[14] };
                patches[9].connections = new List<patch> { patches[8], patches[10], patches[15] };
                patches[10].connections = new List<patch> { patches[1], patches[3], patches[4], patches[7], patches[9], patches[18] };
                patches[11].connections = new List<patch> { patches[5], patches[12], patches[17] };
                patches[12].connections = new List<patch> { patches[6], patches[11], patches[13] };
                patches[13].connections = new List<patch> { patches[7], patches[12], patches[14] };
                patches[14].connections = new List<patch> { patches[8], patches[13], patches[15] };
                patches[15].connections = new List<patch> { patches[9], patches[14], patches[18] };
                patches[16].connections = new List<patch> { patches[17] };
                patches[17].connections = new List<patch> { patches[11], patches[16], patches[18] };
                patches[18].connections = new List<patch> { patches[10], patches[15], patches[17], patches[19] };
                patches[19].connections = new List<patch> { patches[18] };
                break;
            case 4:
                patches[0].connections = new List<patch> { patches[1], patches[4], patches[6], patches[7], patches[11], patches[12], patches[14], patches[16], patches[17], patches[19] };
                patches[1].connections = new List<patch> { patches[0], patches[2] };
                patches[2].connections = new List<patch> { patches[1], patches[3], patches[4] };
                patches[3].connections = new List<patch> { patches[2], patches[5] };
                patches[4].connections = new List<patch> { patches[0], patches[2], patches[5] };
                patches[5].connections = new List<patch> { patches[3], patches[4] };
                patches[6].connections = new List<patch> { patches[0], patches[10] };
                patches[7].connections = new List<patch> { patches[0], patches[8], patches[10] };
                patches[8].connections = new List<patch> { patches[7], patches[9], patches[12] };
                patches[9].connections = new List<patch> { patches[8], patches[10], patches[13] };
                patches[10].connections = new List<patch> { patches[6], patches[7], patches[9], patches[11], patches[14] };
                patches[11].connections = new List<patch> { patches[0], patches[10] };
                patches[12].connections = new List<patch> { patches[0], patches[8], patches[13], patches[15] };
                patches[13].connections = new List<patch> { patches[9], patches[12], patches[14] };
                patches[14].connections = new List<patch> { patches[0], patches[10], patches[13] };
                patches[15].connections = new List<patch> { patches[12], patches[16] };
                patches[16].connections = new List<patch> { patches[0], patches[15] };
                patches[17].connections = new List<patch> { patches[0], patches[18] };
                patches[18].connections = new List<patch> { patches[17], patches[19] };
                patches[19].connections = new List<patch> { patches[0], patches[18] };
                break;
            default:
                throw new System.ArgumentException("puzzleIndex has an invalid value (expected 0-4).");
        }
    }

    private IEnumerable<QColor[]> Recurse(QColor[] sofar)
    {
        var bestIx = -1;
        var mostNeighboringColors = -1;
        for (int i = 0; i < sofar.Length; i++)
        {
            if (sofar[i] != QColor.notSet)
                continue;
            var numNeighboringColors = patches[i].connections.Select(x => x.color).Where(x => x != QColor.notSet).Distinct().Count();
            if (mostNeighboringColors == -1 || numNeighboringColors > mostNeighboringColors)
            {
                bestIx = i;
                mostNeighboringColors = numNeighboringColors;
            }
        }

        if (bestIx == -1)
        {
            yield return sofar.ToArray();
            yield break;
        }
    }

    private class patch
    {
        public int id { get; set; }
        public QColor color { get; set; }
        public List<patch> connections { get; set; }

        public patch(int i, QColor c, List<patch> cn)
        {
            id = i;
            color = c;
            connections = cn;
        }
    }

    private enum QColor
    {
        red,
        yellow,
        blue,
        green,
        notSet
    }

    // Twitch Plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} ";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string input)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        buttons[Array.IndexOf(buttonOrder, solution)].OnInteract();
    }
}
