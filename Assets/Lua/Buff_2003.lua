object:subClass("buff2003")
local battleManager = CS.Tools.Battle.BattleManager.Instance

-- 这些只是配置，不会被运行时使用
buff2003.maxStack = 10
buff2003.lifeTime = 10

-- 【正确】全部使用 . 定义，不要用 :
function buff2003.BuffOnOccur(buff)
-- 【关键】所有属性都从传入的 buff 里取/改
	if buff.CurrentStack < buff.MaxStack then
		buff.CurrentStack = buff.CurrentStack + 1
	end

	print("buff2003 OnOccur: " .. buff.CurrentStack)

	local property = battleManager.Data:get_Item(buff.TargetId)
	local percent = 2 * buff.CurrentStack
	property.CurrentDamage = property.Info.Damage * (1 + percent / 100.0)
end

function buff2003.BuffRemoveOccur(buff)
	print("buff2003 Remove")
end

function buff2003.BuffOnTick(buff, time)
	print("buff2003 OnTick")
	return false
end

function buff2003.BuffOnHit(buff, damageInfo, targetId)
	print("buff2003 OnHit")
end

function buff2003.BuffOnBeHurt(buff, damageInfo, attackerId)
	print("buff2003 OnBeHurt")
end

function buff2003.BuffOnKill(buff, damageInfo, targetId)
	print("buff2003 OnKill")
end

function buff2003.BuffCanRemove(buff, time, force)
	print("buff2003 OnCanRemove")
	return false
end

return buff2003