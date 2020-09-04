namespace TinyUtils.JsInterop.CodeGen.Helper
{
    public abstract class TypedOperand<T> : OperandBase
    {
        public abstract T Reference { get; }
    }
}