namespace Zbyrach.Api
{
    public class Entity
    {
        public long Id { get; set; }

        public override int GetHashCode()
        {
            return (int) Id;
        }

        public override bool Equals(object obj)
        {
            var entity = obj as Entity;
            if (entity == null)
            {
                return false;
            }

            return entity.Id == this.Id;
        }

        public override string ToString()
        {
            return $"{GetType().Name} [{Id}]";
        }
    }
}
