using Villa_API.Models.Dto;

namespace Villa_API.Data
{
    public static class VillaStore
    {
        public static List<VillaDTO> villaList =
        [
            new () { Id=1,Name= "Test 1", SqFt=100,Occupancy=4},
            new () { Id=2,Name= "Test 2", SqFt = 90, Occupancy = 2 }
        ];
    }
}
