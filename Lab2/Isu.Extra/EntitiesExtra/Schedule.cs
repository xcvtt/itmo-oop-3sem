﻿using System.Collections;
using Isu.Extra.Exceptions;

namespace Isu.Extra.EntitiesExtra;

public class Schedule : IEnumerable<Lesson>
{
    private readonly List<Lesson> _lessons;

    public Schedule()
    {
        _lessons = new List<Lesson>();
    }

    public Schedule(IEnumerable<Lesson> lessons)
    {
        ArgumentNullException.ThrowIfNull(lessons);

        _lessons = new List<Lesson>(lessons);
    }

    public Schedule(params Lesson[] lessons)
    {
        ArgumentNullException.ThrowIfNull(lessons);

        _lessons = new List<Lesson>(lessons);
    }

    public IReadOnlyCollection<Lesson> Lessons => _lessons;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<Lesson> GetEnumerator() => _lessons.GetEnumerator();

    internal void AddLesson(Lesson lesson)
    {
        ArgumentNullException.ThrowIfNull(lesson);

        if (_lessons.Contains(lesson))
        {
            throw LessonException.TimeIntersect("Lesson time intersects with the schedule");
        }

        _lessons.Add(lesson);
    }

    internal void RemoveLesson(Lesson lesson) // udalit po vremeni, ne uchitavaya audiotoriu (otdelniy metod v lesson dlya proverki peresch vremeni
    {
        ArgumentNullException.ThrowIfNull(lesson);

        if (!_lessons.Contains(lesson))
        {
            throw new LessonException("Lesson not in the list");
        }

        _lessons.Remove(lesson);
    }
}