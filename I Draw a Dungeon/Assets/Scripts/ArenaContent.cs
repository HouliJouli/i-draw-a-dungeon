using UnityEngine;

public class ArenaContent : MonoBehaviour
{
    [SerializeField] private SpikeWallController spikeWall;
    [SerializeField] private DoorController door;
    [SerializeField] private BoxCollider2D cameraBounds;
    [SerializeField] private BoxCollider2D transitionBounds;

    public SpikeWallController SpikeWall => spikeWall;
    public DoorController Door => door;
    public BoxCollider2D CameraBounds => cameraBounds;
    public BoxCollider2D TransitionBounds => transitionBounds;
}
