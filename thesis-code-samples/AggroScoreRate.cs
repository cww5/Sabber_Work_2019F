public class AggroScore : Score
{
	public override int Rate()
	{
		if (OpHeroHp < 1)
			return int.MaxValue;
		if (HeroHp < 1)
			return int.MinValue;
		int result = 0;
		if (OpBoardZone.Count == 0 && BoardZone.Count > 0)
			result += 1000;
		//Difference lines 13-14
		if (OpMinionTotHealthTaunt > 0)
			result += OpMinionTotHealthTaunt * -1000;
		
		result += MinionTotAtk;
		//Diffence line 18
		result += (HeroHp - OpHeroHp) * 1000;
		return result;
	}
}