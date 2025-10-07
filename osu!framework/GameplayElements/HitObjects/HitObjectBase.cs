using System;
using System.Numerics;

namespace osu.GameplayElements.HitObjects;

public static class Extensions
{
    public static bool IsType(this HitObjectType Type, HitObjectType type)
    {
        return (Type & type) > 0;
    }

    public static bool IsType(this HitObjectSoundType Type, HitObjectSoundType type)
    {
        return (Type & type) > 0;
    }
}

public class HitObjectBase : MarshalByRefObject
{
    /// <summary>
    ///     The zero-based index for which combo colour to use.
    ///     Needed where an RGB triple is insufficient.
    /// </summary>
    public int ComboColourIndex;

    /// <summary>
    ///     If this object has a New Combo marker, how many additional colours do we cycle by?
    /// </summary>
    public int ComboColourOffset = 0;

    /// <summary>
    ///     For spannable objects, the time which the object is no longer valid.
    ///     For non-spannable objects, equal to StartTime.
    /// </summary>
    public int EndTime;

    /// <summary>
    ///     Is this object the last in a combo?
    /// </summary>
    public bool LastInCombo;

    /// <summary>
    ///     Gamefield position of this object.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    ///     The number of segments in this object. As an example, a slider with one repeat arrow will have two segments.
    /// </summary>
    public int SegmentCount = 1;

    /// <summary>
    ///     Hitsound data for this object.
    /// </summary>
    public HitObjectSoundType SoundType;

    /// <summary>
    ///     The length of this object in gamefield pixels.
    /// </summary>
    public double SpatialLength = 0;

    /// <summary>
    ///     Current height in a stack of notes. Zero means no stack.
    /// </summary>
    public int StackCount;

    /// <summary>
    ///     The time at which the object is to be hit.
    /// </summary>
    public int StartTime;

    /// <summary>
    ///     The type of this object.
    /// </summary>
    public HitObjectType Type;

    /// <summary>
    ///     Time length of this object.
    /// </summary>
    public int Length => EndTime - StartTime;

    public bool Whistle
    {
        get => SoundType.IsType(HitObjectSoundType.Whistle);
        set
        {
            if (value)
                SoundType |= HitObjectSoundType.Whistle;
            else
                SoundType &= ~HitObjectSoundType.Whistle;
        }
    }

    public bool Finish
    {
        get => SoundType.IsType(HitObjectSoundType.Finish);
        set
        {
            if (value)
                SoundType |= HitObjectSoundType.Finish;
            else
                SoundType &= ~HitObjectSoundType.Finish;
        }
    }

    public bool Clap
    {
        get => SoundType.IsType(HitObjectSoundType.Clap);
        set
        {
            if (value)
                SoundType |= HitObjectSoundType.Clap;
            else
                SoundType &= ~HitObjectSoundType.Clap;
        }
    }

    /// <summary>
    ///     The length of a segment.
    /// </summary>
    public int SegmentLength => Length / SegmentCount;

    /// <summary>
    ///     Is this object the first in a new combo?
    /// </summary>
    public virtual bool NewCombo
    {
        get => IsType(HitObjectType.NewCombo);
        set
        {
            if (value)
                Type |= HitObjectType.NewCombo;
            else
                Type &= ~HitObjectType.NewCombo;
        }
    }

    /// <summary>
    ///     For spannable objects, the position at which this object ends.
    ///     Note that in the case of repeat sliders, this will be the final position of the ball.
    /// </summary>
    public Vector2 EndPosition { get; set; }

    /// <summary>
    ///     The number displayed on this hitobject (one-based).
    /// </summary>
    public int ComboNumber { get; set; }

    public bool IsType(HitObjectType type)
    {
        return Type.IsType(type);
    }

    public override string ToString()
    {
        return Type + ": " + StartTime + "-" + EndTime + " stack:" + StackCount;
    }

    /// <summary>
    ///     Find the centre of the clickable position at specified time. Useful for sliders, where this position changes.
    /// </summary>
    /// <param name="time"></param>
    /// <returns>Centre of clickable position.</returns>
    public virtual Vector2 PositionAtTime(int time)
    {
        return Position;
    }
}

[Flags]
public enum HitObjectType
{
    Normal = 1,
    Slider = 2,
    NewCombo = 4,
    NormalNewCombo = 5,
    SliderNewCombo = 6,
    Spinner = 8,
    ColourHax = 112,
    Hold = 128,
    ManiaLong = 128
}

[Flags]
public enum HitObjectSoundType
{
    None = 0,
    Normal = 1,
    Whistle = 2,
    Finish = 4,
    Clap = 8
}