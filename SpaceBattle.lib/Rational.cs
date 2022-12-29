namespace SpaceBattle.lib;

public class Rational
{
    int a;
    int b;
    public Rational()
    {
        a = 0;
        b = 1;
    }
    public Rational(int c)
    {
        a = c;
        b = 1;
    }
    public Rational(int c, int d)
    {
        if (d != 0)
        {
        a = c;
        b = d;
        }
        else
        {
            throw new Exception();
        }
    }
    public static Rational operator +(Rational c, Rational d)
    {
        Rational e = new Rational();
        e.b = c.b * d.b;
        e.a = (c.a * d.b) + (d.a * c.b);
        return e;
    }
    public bool Equality(int a, int b)
    {
        if (a == this.a && b == this.b)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}