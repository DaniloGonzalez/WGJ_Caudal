using System;
using Caudal;

// Contrato que usa PopUpController para recibir el resultado del minijuego.
public interface IMinigame
{
    event Action<InteractionResult> OnFinished;
    string ControlsHint { get; }
}
