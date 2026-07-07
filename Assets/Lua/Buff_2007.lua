object:subClass("buff2007")
local battleManager = CS.Tools.Battle.BattleManager.Instance
-- （进行攻击）连续攻击同一目标，每次伤害递增5%（最多30%），永久buff
buff2007.lasttarget = ""
buff2007.damageAddPerc = 0.05
buff2007.damageAddMax = 0.30


function buff2007.BuffRemoveOccur(buff)
	print("buff2007 Remove")
end

function buff2007.BuffOnTick(buff, time)
	print("buff2007 OnTick")
	return false
end

function buff2007.BuffOnHit(buff, damageInfo, targetId)
	print("buff2007 OnHit")
	if buff2007.lasttarget == targetId then
		damageInfo.Damage = damageInfo.Damage * (1 + buff2007.damageAddPerc)
		if damageInfo.Damage > damageInfo.Damage * (1 + buff2007.damageAddMax) then
			damageInfo.Damage = damageInfo.Damage * (1 + buff2007.damageAddMax)
		end
	else
		buff2007.lasttarget = targetId
		damageInfo.Damage = damageInfo.Damage
		end
end

function buff2007.BuffOnBeHurt(buff, damageInfo, attackerId)
	print("buff2007 OnBeHurt")
end

function buff2007.BuffOnKill(buff, damageInfo, targetId)
	print("buff2007 OnKill")
end

function buff2007.BuffCanRemove(buff, time, force)
	print("buff2007 OnCanRemove")
	return false
end

return buff2007