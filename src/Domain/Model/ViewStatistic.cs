using System;

namespace HySite.Domain.Model;

public class ViewStatistic
{
    //@todo: actually, we do not need Id here
    public int Id { get; set; }
    public DateTime Date {get; set;}
    public int Count { get; set; }

    public void Increment() => Count++;
}