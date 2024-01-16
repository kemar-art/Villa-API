using Villa_API.Models.Dto;

namespace Villa_API.Data
{
    public static class VillaStore
    {
        public static List<VillaDTO> villaList =
        [
            new () { Id=1,Name= "Test 1"},
            new () { Id=2,Name= "Test 2"}
        ];
    }
}
