// SerialID: [77a855b2-f53d-4b80-9c94-c40562952b74]
using System;
using UnityEngine;

[Serializable]
public class Matrix
{
    public double this[int r, int c] {
        set {
            Elements[GetIndex(r, c)] = value;
        }
        get {
            return Elements[GetIndex(r, c)];
        }
    }

    [SerializeField] private int row = 0;
    public int Row { get { return row; } private set { row = value; } }

    [SerializeField] private int column = 0;
    public int Column { get { return column; } private set { column = value; } }

    [SerializeField] private double[] elements = null;
    private double[] Elements { get { return elements; } set { elements = value; } }

    public Matrix(int row, int columun) {
        Init(row, columun);
    }

    public Matrix(double[] elements) {
        Init(1, elements.Length);
        for(int c = 0; c < Column; c++) {
            this[0, c] = elements[c];
        }
    }

    public void Init(int row, int column) {
        Row = row;
        Column = column;
        Elements = new double[row * column];
    }

    public int GetIndex(int row, int column) {
        return row * Column + column;
    }

    public Matrix Mul(Matrix m) {
        if(Column != m.Row) {
            throw new ArgumentException("colmun is not match row");
        }

        var newM = new Matrix(Row, m.Column);
        for(int r = 0; r < newM.Row; r++) {
            for(int c = 0; c < newM.Column; c++) {
                newM[r, c] = MulElement(m, r, c);
            }
        }
        return newM;
    }

    public Matrix Copy() {
        var m = new Matrix(Row, Column);
        for(int r = 0; r < Row; r++) {
            for(int c = 0; c < Column; c++) {
                m[r, c] = this[r, c];
            }
        }

        return m;
    }

    public double[] ToArray() {
        return Elements;
    }

    private double MulElement(Matrix m, int index1, int index2) {
        var v = 0.0d;
        for(int c = 0; c < Column; c++) {
            v += this[index1, c] * m[c, index2];
        }

        return v;
    }

    public override string ToString() {
        var str = "";
        for(int r = 0; r < Row; r++) {
            for(int c = 0; c < Column; c++) {
                str += this[r, c] + ",";
            }
            str += "\n";
        }

        return str;
    }

    private Matrix Hadamard(Matrix m) {
        var newM = new Matrix(Row, Column);
        for(int r = 0; r < Row; r++) {
            for(int c = 0; c < Column; c++) {
                newM[r, c] = this[r, c] * m[r, c];
            }
        }
        return newM;
    }
}
