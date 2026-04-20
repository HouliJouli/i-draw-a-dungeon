using Sirenix.OdinInspector;
using UnityEngine;

public class ArenaContent : MonoBehaviour
{
    [BoxGroup("Arena Systems")]
    [SerializeField] private SpikeWallController spikeWall;

    [BoxGroup("Arena Systems")]
    [SerializeField] private DoorController door;

    [BoxGroup("Camera")]
    [Required, SerializeField] private BoxCollider2D cameraBounds;

    [BoxGroup("Camera")]
    [SerializeField] private BoxCollider2D transitionBounds;

    public SpikeWallController SpikeWall => spikeWall;
    public DoorController Door => door;
    public BoxCollider2D CameraBounds => cameraBounds;
    public BoxCollider2D TransitionBounds => transitionBounds;
}
