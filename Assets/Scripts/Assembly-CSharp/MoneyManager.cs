using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour
{
	public Text UIMoney;

	private int money;

	private int oldMoney;

	private bool animated;

	private Color mainColor;

	private AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private void Start()
	{
		if (PlayerPrefs.HasKey("DONATE_COINS"))
		{
			money = PlayerPrefs.GetInt("DONATE_COINS");
		}
		else
		{
			money = 10000;
			PlayerPrefs.SetInt("DONATE_COINS", money);
		}
		if ((bool)UIMoney)
		{
			UIMoney.text = money.ToString();
			mainColor = UIMoney.color;
		}
	}

	public void GiveMoney(int m)
	{
		oldMoney = money;
		money += m;
		UpdateMoney();
	}

	public bool SpendMoney(int m)
	{
		if (money < m)
		{
			StartCoroutine(NoMoney());
			return false;
		}
		oldMoney = money;
		money -= m;
		UpdateMoney();
		return true;
	}

	public bool CheckMoney(int m)
	{
		if (money < m)
		{
			StartCoroutine(NoMoney());
			return false;
		}
		return true;
	}

	private void UpdateMoney()
	{
		if (!animated)
		{
			StartCoroutine(MoneyAnimation());
			animated = true;
		}
		PlayerPrefs.SetInt("DONATE_COINS", money);
	}

	private IEnumerator NoMoney()
	{
		float smooth = 0f;
		while (smooth < 1f)
		{
			smooth += Time.deltaTime * 2f;
			UIMoney.color = Color.Lerp(Color.red, mainColor, smooth);
			yield return null;
		}
	}

	private IEnumerator MoneyAnimation()
	{
		float t = 0f;
		if (money > oldMoney)
		{
			while (oldMoney < money)
			{
				if (money - oldMoney < 10)
				{
					oldMoney++;
				}
				else
				{
					t += Time.deltaTime;
					oldMoney = (int)Mathf.Lerp(oldMoney, money, curve.Evaluate(t / 3f));
				}
				UIMoney.text = oldMoney.ToString();
				yield return null;
			}
		}
		else
		{
			while (oldMoney > money)
			{
				if (oldMoney - money < 10)
				{
					oldMoney--;
				}
				else
				{
					t += Time.deltaTime;
					oldMoney = (int)Mathf.Lerp(oldMoney, money, curve.Evaluate(t / 3f));
				}
				UIMoney.text = oldMoney.ToString();
				yield return null;
			}
		}
		animated = false;
	}
}
