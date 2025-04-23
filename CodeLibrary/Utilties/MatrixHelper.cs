using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using System;
using System.Collections.Generic;

namespace LogSpiralLibrary.CodeLibrary.Utilties
{
    //螺线于2023年初做的非常没软用的东西


    /// <summary>
    /// 还在用这么麻烦的手段实现向量吗？介绍......dynamic!!
    /// </summary>
    /// <typeparam name="TSelf"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface IVector<TSelf, TValue> where TSelf : IVector<TSelf, TValue>
    {
        //T Add(T vector);
        //T Multiply(float scaler);
        //T Divide(float divider) => Multiply(1 / divider);
        TValue Value
        {
            get;
            set;
        }
        TSelf Add(TSelf Another);
        TSelf Subtract(TSelf Another) => Add(Another.Multiply(-1));
        TSelf Multiply(double scaler);
        TSelf Divide(double divider) => Multiply(1 / divider);
    }
    public interface IMatrix<TSelf, TValue> : IVector<TSelf, TValue> where TSelf : IMatrix<TSelf, TValue>
    {
        TSelf Multiply(TSelf Another);
        int Width { get; }
        int Height { get; }
        double[,] Elements { get; }
    }
    public struct FloatArrayVector : IVector<FloatArrayVector, float[]>
    {
        public FloatArrayVector(float[] value)
        {
            Value = value;
        }

        public float[] Value
        {
            get;
            set;
        }
        public float this[int n] => Value[n];
        public FloatArrayVector Add(FloatArrayVector Another)
        {
            if (Value.Length != Another.Value.Length) throw new ArgumentException("向量维度不统一");
            var result = new FloatArrayVector(new float[Value.Length]);
            for (int n = 0; n < Value.Length; n++)
            {
                result.Value[n] = Value[n] + Another[n];
            }
            return result;
        }
        public FloatArrayVector Multiply(double scaler)
        {
            var result = new FloatArrayVector(new float[Value.Length]);
            for (int n = 0; n < Value.Length; n++)
            {
                result.Value[n] = Value[n] * (float)scaler;
            }
            return result;
        }
    }
    public struct FloatVector1 : IVector<FloatVector1, float>
    {

        public FloatVector1(float value)
        {
            Value = value;
        }

        public float Value
        {
            get;
            set;
        }


        public FloatVector1 Add(FloatVector1 Another)
        {
            return new FloatVector1(Value + Another.Value);
        }
        public FloatVector1 Multiply(double scaler)
        {
            return new FloatVector1(Value * (float)scaler);
        }
    }
    public struct FloatVector2 : IVector<FloatVector2, Vector2>
    {

        public FloatVector2(Vector2 value)
        {
            Value = value;
        }
        public float this[int n] => n switch
        {
            0 => Value.X,
            1 => Value.Y,
            _ => 0
        };
        public Vector2 Value
        {
            get;
            set;
        }

        public FloatVector2 Add(FloatVector2 Another)
        {
            return new FloatVector2(Value + Another.Value);
        }
        public FloatVector2 Multiply(double scaler)
        {
            return new FloatVector2(Value * (float)scaler);
        }
    }
    public struct FloatVector3 : IVector<FloatVector3, Vector3>
    {

        public FloatVector3(Vector3 value)
        {
            Value = value;
        }

        public Vector3 Value
        {
            get;
            set;
        }
        public float this[int n] => n switch
        {
            0 => Value.X,
            1 => Value.Y,
            2 => Value.Z,
            _ => 0
        };
        public FloatVector3 Add(FloatVector3 Another)
        {
            return new FloatVector3(Value + Another.Value);
        }
        public FloatVector3 Multiply(double scaler)
        {
            return new FloatVector3(Value * (float)scaler);
        }
    }
    public struct FloatVector4 : IVector<FloatVector4, Vector4>
    {

        public FloatVector4(Vector4 value)
        {
            Value = value;
        }

        public Vector4 Value
        {
            get;
            set;
        }
        public float this[int n] => n switch
        {
            0 => Value.X,
            1 => Value.Y,
            2 => Value.Z,
            3 => Value.W,
            _ => 0
        };
        public FloatVector4 Add(FloatVector4 Another)
        {
            return new FloatVector4(Value + Another.Value);
        }
        public FloatVector4 Multiply(double scaler)
        {
            return new FloatVector4(Value * (float)scaler);
        }
    }
    public struct ColorVector : IVector<ColorVector, Color>
    {

        public ColorVector(Color value)
        {
            Value = value;
        }

        public Color Value
        {
            get;
            set;
        }
        public byte this[int n] => n switch
        {
            0 => Value.R,
            1 => Value.G,
            2 => Value.B,
            3 => Value.A,
            _ => 0
        };
        public ColorVector Add(ColorVector Another)
        {
            var c1 = Value;
            var c2 = Another.Value;
            var vector = c1.ToVector4() + c2.ToVector4();
            return new ColorVector(new Color(vector));
        }
        public ColorVector Multiply(double scaler)
        {
            return new ColorVector(Value * (float)scaler);
        }
    }
    public struct Matrix4X4 : IMatrix<Matrix4X4, Matrix>
    {
        public Matrix4X4(Matrix value)
        {
            Value = value;
        }
        public int Width => 4;

        public int Height => 4;

        /// <summary>
        /// 只读，不要从这里写入数据
        /// </summary>
        public double[,] Elements
        {
            get
            {
                var result = new double[4, 4];
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        var m = Value;
                        result[i, j] =
                            i switch
                            {
                                0 => j switch
                                {
                                    0 => m.M11,
                                    1 => m.M12,
                                    2 => m.M13,
                                    3 or _ => m.M14
                                },
                                1 => j switch
                                {
                                    0 => m.M21,
                                    1 => m.M22,
                                    2 => m.M23,
                                    3 or _ => m.M24
                                },
                                2 => j switch
                                {
                                    0 => m.M31,
                                    1 => m.M32,
                                    2 => m.M33,
                                    3 or _ => m.M34
                                },
                                3 or _ => j switch
                                {
                                    0 => m.M41,
                                    1 => m.M42,
                                    2 => m.M43,
                                    3 or _ => m.M44
                                }
                            };
                    }
                }
                return result;
            }
        }

        public Matrix Value { get; set; }

        public Matrix4X4 Add(Matrix4X4 Another)
        {
            return new Matrix4X4(Value + Another.Value);
        }

        public Matrix4X4 Multiply(Matrix4X4 Another)
        {
            return new Matrix4X4(Another.Value * Value);
        }

        public Matrix4X4 Multiply(double scaler)
        {
            return new Matrix4X4(Value * (float)scaler);
        }
    }
    /// <summary>
    /// 以float为元素的大矩阵
    /// </summary>
    public class MatrixEX : IMatrix<MatrixEX, MatrixEX>
    {
        public override string ToString()
        {
            var str = "(";
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {

                    str += $" {this[i, j]},";
                }
                if (i != Height - 1)
                    str += "    ";
            }
            str += ")";
            return str;
        }
        public double[,] Elements => elements;
        /// <summary>
        /// 这个是为其它实现了这个接口的类服务的，请忽视
        /// </summary>
        public MatrixEX Value { get => this; set => throw new Exception("不要用这个Value——————"); }
        /// <summary>
        /// 矩阵的元素们！
        /// </summary>
        public double[,] elements;
        /// <summary>
        /// 矩阵的宽度，或者说列的数量
        /// </summary>
        public int Width => elements.GetLength(1);
        /// <summary>
        /// 矩阵的高度，或者说行的数量
        /// </summary>
        public int Height => elements.GetLength(0);
        /// <summary>
        /// 最暴力的直接把二维数组打包成矩阵的构造函数！
        /// </summary>
        /// <param name="value">矩阵的元素们</param>
        public MatrixEX(double[,] value)
        {
            elements = value;
        }
        /// <summary>
        /// 指定高宽然后空矩阵
        /// </summary>
        /// <param name="height">高</param>
        /// <param name="width">宽</param>
        public MatrixEX(int height, int width)
        {
            elements = new double[height, width];
        }
        /// <summary>
        /// 指定阶然后空矩阵
        /// </summary>
        /// <param name="tier">阶</param>
        public MatrixEX(int tier) : this(tier, tier)
        {
        }
        /// <summary>
        /// 指定高宽然后序列生成
        /// </summary>
        /// <param name="height">高</param>
        /// <param name="width">宽</param>
        /// <param name="func">喜欢我二维数列吗</param>
        public MatrixEX(int height, int width, Func<int, int, double> func)
        {
            elements = new double[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    elements[i, j] = func(i, j);
        }
        /// <summary>
        /// 指定阶然后序列生成
        /// </summary>
        /// <param name="tier">阶</param>
        /// <param name="func">喜欢我二维数列吗</param>
        public MatrixEX(int tier, Func<int, int, double> func) : this(tier, tier, func)
        {
        }
        /// <summary>
        /// 获取第i列第j行的元素
        /// </summary>
        /// <param name="i">列标</param>
        /// <param name="j">行标</param>
        /// <returns>返回m_ij</returns>
        public double this[int i, int j]
        {
            get
            {
                if (i >= Height || i < 0)
                {
                    string text = $"行标出错，行数为{Height}，传入为{i}";
                    Main.NewText(text);
                    throw new ArgumentException(text);
                }
                if (j >= Width || j < 0)
                {
                    string text = $"列标出错，列数为{Width}，传入为{j}";
                    Main.NewText(text);
                    throw new ArgumentException(text);
                }
                return elements[i, j];
            }
            set => elements[i, j] = value;
        }
        /// <summary>
        /// 两个矩阵作加法
        /// </summary>
        /// <param name="Another">加上的那个矩阵</param>
        /// <returns>返回加和结果！这个操作是可交换的</returns>
        /// <exception cref="ArgumentException"></exception>
        public MatrixEX Add(MatrixEX Another)
        {
            if (Width != Another.Width) throw new ArgumentException($"兄啊，你的宽度不对劲！原宽度为{Width}，另一个矩阵的宽度为{Another.Width}");
            if (Height != Another.Height) throw new ArgumentException($"兄啊，你的高度不对劲！原高度为{Height}，另一个矩阵的高度为{Another.Height}");
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    elements[i, j] += Another[i, j];
                }
            }
            return this;
        }
        /// <summary>
        /// 矩阵数乘
        /// </summary>
        /// <param name="scaler">倍率</param>
        /// <returns>每个元素都乘上了scaler，几乎没什么好注意的很普通的函数</returns>
        public MatrixEX Multiply(double scaler)
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    elements[i, j] *= scaler;
                }
            }
            FloatVector1[] vecs = new FloatVector1[4];
            for (int n = 0; n < 4; n++)
            {
                vecs[n] = new FloatVector1(n * n + 1);
            }
            return this;
        }
        /// <summary>
        /// 将两个矩阵作乘法
        /// </summary>
        /// <param name="matrix">左乘矩阵</param>
        /// <returns>目标矩阵左乘这个矩阵</returns>
        /// <exception cref="ArgumentException"></exception>
        public MatrixEX Multiply(MatrixEX matrix)
        {
            int times = Height;
            if (times != matrix.Width) throw new ArgumentException($"左矩阵的高度应该与右矩阵的宽度相等，左高度为{times}，右宽度为{matrix.Width}");
            int width = Width;
            int height = matrix.Height;
            var result = new MatrixEX(height, width);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    for (int m = 0; m < times; m++)
                        result[i, j] += matrix[i, m] * this[m, j];
            return result;
        }
        /// <summary>
        /// 获取余子式
        /// </summary>
        /// <param name="i">从第几列划去</param>
        /// <param name="j">从第几行划去</param>
        /// <returns>返回鱼籽柿！</returns>
        public MatrixEX Cofactor(int i, int j)
        {
            int height = Height;
            int width = Width;
            if (height == 1 || width == 1) throw new ArgumentException("长或宽为1的无法求余子式");
            var result = new MatrixEX(height - 1, width - 1);
            bool flag1 = false;
            for (int m = 0; m < height - 1; m++)
            {
                bool flag2 = false;
                if (m == i) flag1 = true;
                for (int n = 0; n < width - 1; n++)
                {
                    if (n == j) flag2 = true;
                    result[m, n] = this[m + (flag1 ? 1 : 0), n + (flag2 ? 1 : 0)];
                }
            }
            return result;
        }
        /// <summary>
        /// 求当前矩阵的行列式
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public double Determinant()
        {
            if (Width != Height) throw new ArgumentException($"只有方阵能求行列式，当前高宽分别为{Height}和{Width}");
            if (Width == 1) return this[0, 0];
            if (Width == 2) return this[0, 0] * this[1, 1] - this[1, 0] * this[0, 1];
            var result = 0.0;
            for (int n = 0; n < Height; n++)
            {
                result += (n % 2 == 0 ? 1 : -1) * this[n, 0] * Cofactor(n, 0).Determinant();
            }
            return result;
        }
        /// <summary>
        /// 求当前矩阵的转置矩阵
        /// </summary>
        /// <returns>交换行和列</returns>
        public MatrixEX Transposition()
        {
            int height = Height;
            int width = Width;
            var result = new MatrixEX(width, height);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    result[j, i] = this[i, j];
            return result;
        }
        /// <summary>
        /// 求当前矩阵的伴随矩阵
        /// </summary>
        /// <returns></returns>
        public MatrixEX Adjugate()
        {
            if (Width != Height) throw new ArgumentException($"只有方阵能求伴随矩阵，当前高宽分别为{Height}和{Width}");
            int height = Height;
            int width = Width;
            var result = new MatrixEX(height, width);
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    var cofactor = Cofactor(i, j);
                    var det = cofactor.Determinant();
                    result[i, j] = ((i + j) % 2 == 0 ? 1 : -1) * det;
                }
            return result.Transposition();
        }
        /// <summary>
        /// 求当前矩阵的逆矩阵
        /// </summary>
        /// <returns></returns>
        public MatrixEX Inverse()
        {
            if (Width != Height) throw new ArgumentException($"只有方阵能求逆矩阵，当前高宽分别为{Height}和{Width}");
            if (Width == 1)
            {
                var result = new MatrixEX(1);
                result[0, 0] = 1 / this[0, 0];
                return result;
            }
            var adjugate = Adjugate();
            var determinant = Determinant();
            return adjugate / determinant;
        }
        /// <summary>
        /// 按行拓展矩阵
        /// </summary>
        /// <param name="Left">接在左边</param>
        /// <param name="Right">接在右边</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static MatrixEX AppendByRow(MatrixEX Left, MatrixEX Right)
        {
            if (Left.Height != Right.Height) throw new ArgumentException($"两个矩阵的高度(行数)需要相等，目前分别是{Left.Height}和{Right.Height}");
            int width = Left.Width;
            var result = new MatrixEX(Left.Height, width + Right.Width,
                (i, j) =>
                {
                    if (j < width)
                    {
                        return Left[i, j];
                    }
                    return Right[i, j - width];
                }
                );
            return result;
        }
        /// <summary>
        /// 按列拓展矩阵
        /// </summary>
        /// <param name="Up">接在上边</param>
        /// <param name="Down">接在下边</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static MatrixEX AppendByColumn(MatrixEX Up, MatrixEX Down)
        {
            if (Up.Width != Down.Width) throw new ArgumentException($"两个矩阵的宽度(列数)需要相等，目前分别是{Up.Width}和{Down.Width}");
            int height = Up.Height;
            var result = new MatrixEX(height + Down.Height, Up.Width,
                (i, j) =>
                {
                    if (i < height)
                    {
                        return Up[i, j];
                    }
                    return Down[i - height, j];
                }
                );
            return result;
        }
        public static MatrixEX operator *(MatrixEX left, MatrixEX right)
        {
            return right.Multiply(left);
        }
        public static MatrixEX operator *(MatrixEX matrix, double scaler)
        {
            return matrix.Multiply(scaler);
        }
        public static MatrixEX operator *(double scaler, MatrixEX matrix)
        {
            return matrix.Multiply(scaler);
        }
        public static MatrixEX operator +(MatrixEX left, MatrixEX right)
        {
            return left.Add(right);
        }
        public static MatrixEX operator -(MatrixEX matrix)
        {
            var result = new MatrixEX(matrix.Height, matrix.Width,
                (i, j) => matrix[i, j]);
            return result.Multiply(-1);
        }
        public static MatrixEX operator -(MatrixEX left, MatrixEX right)
        {
            return left.Add(-right);
        }
        public static MatrixEX operator /(MatrixEX matrix, double scaler)
        {
            return matrix.Multiply(1 / scaler);
        }

        public IVector<TSelf, TValue>[] Apply<TSelf, TValue>(IVector<TSelf, TValue>[] array1, params IVector<TSelf, TValue>[] array2) where TSelf : IVector<TSelf, TValue>
        {
            var array = new IVector<TSelf, TValue>[array1.Length + array2.Length];
            if (array.Length != Width) throw new ArgumentException($"向量的维数必须和矩阵的宽度相等，目前分别为{array.Length}和{Width}");
            for (int n = 0; n < array1.Length; n++)
                array[n] = array1[n];
            for (int n = 0; n < array2.Length; n++)
                array[n + array1.Length] = array2[n];
            var result = new IVector<TSelf, TValue>[Height];
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    if (j == 0) result[i] = array[0].Multiply(this[i, 0]);
                    else result[i] = result[i].Add(array[j].Multiply(this[i, j]));
            return result;
        }
        /*
        public IVector<TSelf, TValue>[,] Apply<TSelf, TValue>(IVector<TSelf, TValue>[,] array) where TSelf : IVector<TSelf, TValue>
        {
            var length = array.GetLength(0);
            if (length != Width) throw new ArgumentException($"右矩阵的行数必须和左矩阵的列数相等，目前分别为{length}和{Width}");
            int width = array.GetLength(1);
            int height = Height;
            var result = new IVector<TSelf, TValue>[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    for (int m = 0; m < length; m++)
                        if (m == 0) result[i, j] = array[0, j].Multiply(this[i, 0]);
                        else result[i, j] = result[i, j].Add(array[m, j].Multiply(this[i, m]));
            return result;
        }

        public double[] Apply(double[] array1, params double[] array2)
        {
            var array = new double[array1.Length + array2.Length];
            if (array.Length != Width) throw new ArgumentException($"向量的维数必须和矩阵的宽度相等，目前分别为{array.Length}和{Width}");
            for (int n = 0; n < array1.Length; n++)
                array[n] = array1[n];
            for (int n = 0; n < array2.Length; n++)
                array[n + array1.Length] = array2[n];
            var result = new double[Height];
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    if (j == 0) result[i] = array[0] * this[i, 0];
                    else result[i] = result[i] + array[j] * this[i, j];
            return result;
        }

        public Vector2[] Apply(Vector2[] array1, params Vector2[] array2)
        {
            var array = new Vector2[array1.Length + array2.Length];
            if (array.Length != Width) throw new ArgumentException($"向量的维数必须和矩阵的宽度相等，目前分别为{array.Length}和{Width}");
            for (int n = 0; n < array1.Length; n++)
                array[n] = array1[n];
            for (int n = 0; n < array2.Length; n++)
                array[n + array1.Length] = array2[n];
            var result = new Vector2[Height];
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    if (j == 0) result[i] = array[0] * this[i, 0];
                    else result[i] += array[j] * this[i, j];
            return result;
        }

        public Vector3[] Apply(Vector3[] array1, params Vector3[] array2)
        {
            var array = new Vector3[array1.Length + array2.Length];
            if (array.Length != Width) throw new ArgumentException($"向量的维数必须和矩阵的宽度相等，目前分别为{array.Length}和{Width}");
            for (int n = 0; n < array1.Length; n++)
                array[n] = array1[n];
            for (int n = 0; n < array2.Length; n++)
                array[n + array1.Length] = array2[n];
            var result = new Vector3[Height];
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    if (j == 0) result[i] = array[0] * this[i, 0];
                    else result[i] = result[i] + array[j] * this[i, j];
            return result;
        }

        public Vector4[] Apply(Vector4[] array1, params Vector4[] array2)
        {
            var array = new Vector4[array1.Length + array2.Length];
            if (array.Length != Width) throw new ArgumentException($"向量的维数必须和矩阵的宽度相等，目前分别为{array.Length}和{Width}");
            for (int n = 0; n < array1.Length; n++)
                array[n] = array1[n];
            for (int n = 0; n < array2.Length; n++)
                array[n + array1.Length] = array2[n];
            var result = new Vector4[Height];
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    if (j == 0) result[i] = array[0] * this[i, 0];
                    else result[i] = result[i] + array[j] * this[i, j];
            return result;
        }
        */
        public double[,] Apply(double[,] array)
        {
            var length = array.GetLength(0);
            if (length != Width) throw new ArgumentException($"右矩阵的行数必须和左矩阵的列数相等，目前分别为{length}和{Width}");
            int width = array.GetLength(1);
            int height = Height;
            var result = new double[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    for (int m = 0; m < length; m++)
                        if (m == 0) result[i, j] = array[0, j] * this[i, 0];
                        else result[i, j] = result[i, j] + array[m, j] * this[i, m];
            return result;
        }
        /*


        public Vector2[,] Apply(Vector2[,] array)
        {
            var length = array.GetLength(0);
            if (length != Width) throw new ArgumentException($"右矩阵的行数必须和左矩阵的列数相等，目前分别为{length}和{Width}");
            int width = array.GetLength(1);
            int height = Height;
            var result = new Vector2[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    for (int m = 0; m < length; m++)
                        if (m == 0) result[i, j] = array[0, j] * this[i, 0];
                        else result[i, j] = result[i, j] + array[m, j] * this[i, m];
            return result;
        }
        public Vector3[,] Apply(Vector3[,] array)
        {
            var length = array.GetLength(0);
            if (length != Width) throw new ArgumentException($"右矩阵的行数必须和左矩阵的列数相等，目前分别为{length}和{Width}");
            int width = array.GetLength(1);
            int height = Height;
            var result = new Vector3[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    for (int m = 0; m < length; m++)
                        if (m == 0) result[i, j] = array[0, j] * this[i, 0];
                        else result[i, j] = result[i, j] + array[m, j] * this[i, m];
            return result;
        }
        public Vector4[,] Apply(Vector4[,] array)
        {
            var length = array.GetLength(0);
            if (length != Width) throw new ArgumentException($"右矩阵的行数必须和左矩阵的列数相等，目前分别为{length}和{Width}");
            int width = array.GetLength(1);
            int height = Height;
            var result = new Vector4[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    for (int m = 0; m < length; m++)
                        if (m == 0) result[i, j] = array[0, j] * this[i, 0];
                        else result[i, j] = result[i, j] + array[m, j] * this[i, m];
            return result;
        }

        */
    }
    public static class BezierCurveHelper
    {
        /// <summary>
        /// 系数向量，只在第一次使用的时候生成，是固定值   ps:不要自己赋值————
        /// </summary>
        static List<double[]> c_Vectors = [];
        /// <summary>
        /// 求点矩阵(M^-1*[I -T])，只在第一次使用的时候生成，是固定值   ps:不要自己赋值————
        /// </summary>
        static List<MatrixEX> c_Matrixes = [];
        /// <summary>
        /// 是否可用
        /// </summary>
        static List<bool> active = [];

        public static void ConstCheck(int n)
        {
            int hasValueCount = active.Count;
            if (hasValueCount >= n && active[n - 1])
                return;
            for (int m = 0; m < n; m++)
            {
                if (m >= hasValueCount)
                {
                    if (m == n - 1)
                    {
                        c_Vectors.Add(GetVectors(m));
                        c_Matrixes.Add(GetMatrix(m));
                        active.Add(true);
                    }
                    else
                    {
                        c_Vectors.Add(null);
                        c_Matrixes.Add(null);
                        active.Add(false);
                    }
                }
                else if (m == n - 1)
                {
                    c_Vectors[m] = GetVectors(m);
                    c_Matrixes[m] = GetMatrix(m);
                    active[m] = true;
                }
            }
        }

        public static MatrixEX GetMatrix(int n)
        {
            if (c_Matrixes.Count > n) return c_Matrixes[n];
            MatrixEX M = new(n + 1,
                (i, j) =>
                {
                    double t = (1 + i) / (n + 2f);
                    return Math.Pow(1 - t, n - j + 1) * Math.Pow(t, j + 1) * c_Vectors[n][j + 1];
                }
                );
            MatrixEX T = new(n + 1, 2,
                (i, j) =>
                {
                    double t = (1 + i) / (n + 2f);
                    if (j == 0)
                    {
                        t = 1 - t;
                    }
                    return Math.Pow(t, n + 2);
                }
                );
            M = M.Inverse();
            return MatrixEX.AppendByRow(M, -M * T);
        }
        public static double[] GetVectors(int n)
        {
            if (c_Vectors.Count > n) return c_Vectors[n];
            n += 3;
            var result = new double[n];
            for (int k = 0; k < n; k++)
            {
                int r = MathMethods.Combination(n - 1, k);
                result[k] = r;
            }
            return result;
        }
    }
    /// <summary>
    /// <param>贝塞尔曲线类！！</param>
    /// <param>我知道两个类型参数很蠢，明明一个就够的，但是没办法呜呜</param>
    /// </summary>
    /// <typeparam name="TSelf">对应的向量类型</typeparam>
    /// <typeparam name="TValue">原类型</typeparam>
    public class BezierCurve<TSelf, TValue> where TSelf : IVector<TSelf, TValue>
    {

        /// <summary>
        /// 控制点
        /// </summary>
        public IVector<TSelf, TValue>[] vectors;
        /// <summary>
        /// 平滑结果，数量或者控制点变了的时候才变
        /// </summary>
        public IVector<TSelf, TValue>[] results;

        public BezierCurve(IVector<TSelf, TValue>[] input)
        {
            int n = input.Length;
            if (n < 3) vectors = input;
            n -= 2;
            BezierCurveHelper.ConstCheck(n);
            vectors = new IVector<TSelf, TValue>[n + 2];
            vectors[0] = input[0];
            vectors[^1] = input[^1];
            IVector<TSelf, TValue>[] array = BezierCurveHelper.GetMatrix(n - 1).Apply(input[1..^1], input[0], input[^1]);
            for (int m = 0; m < n; m++)
                vectors[m + 1] = array[m];
        }
        /// <summary>
        /// (重新)计算结果
        /// </summary>
        /// <param name="count">采样数，需要大于1，从头走到尾</param>
        public void Recalculate(int count)
        {
            if (count < 2) throw new ArgumentException("兄啊，一个点都没有插值个锤子");
            results = new IVector<TSelf, TValue>[count];
            int tier = vectors.Length;
            if (tier == 1)
            {
                for (int n = 0; n < count; n++) results[n] = vectors[0];
                return;
            }
            if (tier == 2)
            {
                for (int n = 0; n < count; n++)
                {
                    float t = n / (count - 1f);
                    results[n] = vectors[0].Multiply(1 - t).Add(vectors[1].Multiply(t));
                }
                return;
            }
            for (int n = 0; n < count; n++)
            {
                float t = n / (count - 1f);
                for (int i = 0; i < tier; i++)
                {
                    if (i == 0) results[n] = vectors[0].Multiply(MathF.Pow(1 - t, tier - 1));
                    else results[n] = results[n].Add(vectors[i].Multiply(MathF.Pow(1 - t, tier - i - 1) * MathF.Pow(t, i) * BezierCurveHelper.GetVectors(tier - 3)[i]));
                }
            }
        }
    }
}
