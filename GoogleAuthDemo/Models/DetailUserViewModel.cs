using Microsoft.Graph.Models;

namespace GoogleAuthDemo.Models
{
    public class DetailUserViewModel
    {
        public User User { get; set; }
        public List<IdentityUserFlowAttribute> CustomAttributes { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }

    }
}
