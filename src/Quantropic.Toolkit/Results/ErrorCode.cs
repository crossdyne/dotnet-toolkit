namespace Quantropic.Toolkit.Results
{
    public readonly record struct ErrorCode
    {
        public readonly string Name { get; }
        public readonly int Code { get; }

        private ErrorCode(string name, int code)
        {
            Name = name;
            Code = code;
        }

        public static readonly ErrorCode None = new(nameof(None), 0);
        public static readonly ErrorCode NotFound = new(nameof(NotFound), 0);
        public static readonly ErrorCode Null = new(nameof(Null), 0);
        public static readonly ErrorCode NullValue = new(nameof(NullValue), 0);
        public static readonly ErrorCode Empty = new(nameof(Empty), 0);
        public static readonly ErrorCode EmptyValue = new(nameof(EmptyValue), 0);

        public static readonly ErrorCode Exist = new(nameof(Exist), 0);
        public static readonly ErrorCode NotExists = new(nameof(NotExists), 0);

        public static readonly ErrorCode Invalid = new(nameof(Invalid), 0);
        public static readonly ErrorCode InvalidType = new(nameof(InvalidType), 0);
        public static readonly ErrorCode InvalidValue = new(nameof(InvalidValue), 0);
        public static readonly ErrorCode InvalidResponse = new(nameof(InvalidResponse), 0);
        public static readonly ErrorCode InvalidRequest = new(nameof(InvalidRequest), 0);

        public static readonly ErrorCode Create = new(nameof(Create), 0);
        public static readonly ErrorCode Update = new(nameof(Update), 0);
        public static readonly ErrorCode Delete = new(nameof(Delete), 0);
        public static readonly ErrorCode Save = new(nameof(Save), 0);
        public static readonly ErrorCode Conflict = new(nameof(Conflict), 0);
        public static readonly ErrorCode Connection = new(nameof(Connection), 0);
        public static readonly ErrorCode Server = new(nameof(Server), 0);
        public static readonly ErrorCode BadRequest = new(nameof(BadRequest), 0);
        public static readonly ErrorCode Unauthorized = new(nameof(Unauthorized), 0);
    }
}