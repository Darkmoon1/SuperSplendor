using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Pers.YHY.SuperSplendor
{
	public class CamaraFix : MonoBehaviour
	{
		public Camera mainCamera;
		void Start()
		{
			//Screen.SetResolution(1280, 800, true, 60);
			mainCamera = Camera.main;
			//  float screenAspect = 1280 / 720;  ����android�ֻ��������ֱ档
			//  mainCamera.aspect --->  ������ĳ���ȣ���ȳ��Ը߶ȣ�
			mainCamera.aspect = 2.16f;
		}
	}

}