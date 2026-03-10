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

        // 0 – a special error-free code
        public static readonly ErrorCode None = new(nameof(None), 0);

        // 1–99: Common errors
        public static readonly ErrorCode NotFound   = new(nameof(NotFound), 1);
        public static readonly ErrorCode Null       = new(nameof(Null), 2);
        public static readonly ErrorCode NullValue  = new(nameof(NullValue), 3);
        public static readonly ErrorCode Empty      = new(nameof(Empty), 4);
        public static readonly ErrorCode EmptyValue = new(nameof(EmptyValue), 5);
        public static readonly ErrorCode Exist      = new(nameof(Exist), 6);
        public static readonly ErrorCode NotExists  = new(nameof(NotExists), 7); 

        // 100–199: Validation errors
        public static readonly ErrorCode Invalid        = new(nameof(Invalid), 100);
        public static readonly ErrorCode InvalidType    = new(nameof(InvalidType), 101);
        public static readonly ErrorCode InvalidValue   = new(nameof(InvalidValue), 102);
        public static readonly ErrorCode InvalidResponse = new(nameof(InvalidResponse), 103);
        public static readonly ErrorCode InvalidRequest = new(nameof(InvalidRequest), 104);

        // 200–299: Operation errors (CRUD and save)
        public static readonly ErrorCode Create = new(nameof(Create), 200);
        public static readonly ErrorCode Update = new(nameof(Update), 201);
        public static readonly ErrorCode Delete = new(nameof(Delete), 202);
        public static readonly ErrorCode Save   = new(nameof(Save), 203);

        // 300–399: Interaction errors (network, server, authorization)
        public static readonly ErrorCode Conflict     = new(nameof(Conflict), 300);
        public static readonly ErrorCode Connection   = new(nameof(Connection), 301);
        public static readonly ErrorCode Server       = new(nameof(Server), 302);
        public static readonly ErrorCode BadRequest   = new(nameof(BadRequest), 303);
        public static readonly ErrorCode Unauthorized = new(nameof(Unauthorized), 304);

        /// <summary>
        ///  User code > 10.000
        /// </summary>
        /// <param name="name"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static ErrorCode Custom(string name, int code)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));

            if (code < 10_000)
                throw new ArgumentOutOfRangeException(nameof(code), "Custom error code must be 10000 or greater.");

            return new ErrorCode(name, code);
        }
    }
}