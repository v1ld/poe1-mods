using Patchwork.Attributes;
using UnityEngine;

namespace V1ldBetterFollowCamera
{
    [ModifiesType]
    class V1ld_CameraControl : CameraControl
    {
        [NewMember]
        private bool m_followMode;

        [NewMember]
        private void ReInitFollow()
        {
            if (!m_followMode)
            {
                return;
            }

            m_FollowingUnits.Clear();
            for (int i = 0; i < PartyMemberAI.SelectedPartyMembers.Length; i++)
            {
                if ((bool)PartyMemberAI.SelectedPartyMembers[i])
                {
                    m_FollowingUnits.Add(PartyMemberAI.SelectedPartyMembers[i]);
                }
            }
        }

        [ModifiesMember("DoUpdate")]
        new public void DoUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            Camera main = Camera.main;
            if (m_forceReset)
            {
                m_testLeft = true;
                m_testRight = true;
                m_testTop = true;
                m_testBottom = true;
                m_forceReset = false;
            }
            else
            {
                m_testLeft = false;
                m_testRight = false;
                m_testTop = false;
                m_testBottom = false;
            }
            if (PlayerControlEnabled && GameState.ApplicationIsFocused)
            {
                if (PlayerScrollEnabled)
                {
                    float axisRaw = Input.GetAxisRaw("Mouse ScrollWheel");
                    if (axisRaw != 0f)
                    {
                        OrthoSettings.SetZoomLevelDelta(axisRaw);
                        ResetAtEdges();
                    }
                }
                if (GameInput.GetDoublePressed(KeyCode.Mouse0, handle: true) && !UINoClick.MouseOverUI)
                {
                    // v1ld: map double click, disable follow mode
                    CancelFollow();
                    Vector3 point = GameInput.WorldMousePosition;
                    if ((bool)GameCursor.CharacterUnderCursor)
                    {
                        point = GameCursor.CharacterUnderCursor.transform.position;
                        ResetAtEdges();
                    }
                    if ((bool)Instance)
                    {
                        Instance.FocusOnPoint(point, 0.4f);
                        ResetAtEdges();
                    }
                }
                if (GameInput.GetControlDownWithRepeat(MappedControl.ZOOM_IN, handle: true))
                {
                    OrthoSettings.SetZoomLevelDelta(m_zoomRes);
                    ResetAtEdges();
                }
                if (GameInput.GetControlDownWithRepeat(MappedControl.ZOOM_OUT, handle: true))
                {
                    OrthoSettings.SetZoomLevelDelta(0f - m_zoomRes);
                    ResetAtEdges();
                }
                if (GameInput.GetControlUp(MappedControl.RESET_ZOOM))
                {
                    OrthoSettings.SetZoomLevel(1f, force: false);
                    ResetAtEdges();
                }
                // v1ld: if we're in follow mode, re-populate the units list on any action
                // where the player may want the camera to move back to the party
                if (m_followMode &&
                      (GameInput.GetControlUp(MappedControl.MOVE)
                    || GameInput.GetControlUp(MappedControl.INTERACT)
                    //|| GameInput.GetControlUp(MappedControl.ATTACK)
                    //|| GameInput.GetControlUp(MappedControl.STEALTH_TOGGLE)
                    //|| GameInput.GetControlUp(MappedControl.STEALTH_ON)
                    //|| GameInput.GetControlUp(MappedControl.STEALTH_OFF)
                    || GameInput.GetControlUp(MappedControl.SELECT)
                    || GameInput.GetControlUp(MappedControl.SELECT_ALL)
                    || GameInput.GetControlUp(MappedControl.MULTISELECT)))
                {   
                    ReInitFollow();
                }
                if (GameInput.GetControlUp(MappedControl.FOLLOW_CAM))
                {
                    // v1ld: 
                    m_followMode = !m_followMode;
                    if (m_followMode)
                    {
                        ReInitFollow();
                    }
                    else
                    {
                        CancelFollow();
                    }
                }
                if (GameInput.GetControlDown(MappedControl.PAN_CAMERA))
                {
                    m_mouseDrag_lastMousePos = GameInput.MousePosition;
                    float num = main.pixelWidth * 0.5f;
                    float num2 = main.pixelHeight * 0.5f;
                    Vector3 vector = ProjectScreenCoordsToGroundPlane(main, new Vector3(num + 1f, num2, main.nearClipPlane));
                    Vector3 vector2 = ProjectScreenCoordsToGroundPlane(main, new Vector3(num, num2, main.nearClipPlane));
                    CameraPanDeltaX = vector2 - vector;
                    vector = ProjectScreenCoordsToGroundPlane(main, new Vector3(num, num2 + 1f, main.nearClipPlane));
                    vector2 = ProjectScreenCoordsToGroundPlane(main, new Vector3(num, num2, main.nearClipPlane));
                    CameraPanDeltaY = vector2 - vector;
                }
                else if (GameInput.GetControl(MappedControl.PAN_CAMERA))
                {
                    CancelFollow();
                    Vector3 vector3 = GameInput.MousePosition - m_mouseDrag_lastMousePos;
                    m_mouseDrag_lastMousePos = GameInput.MousePosition;
                    if (vector3.x < 0f)
                    {
                        m_atLeft = false;
                    }
                    else if (vector3.x > 0f)
                    {
                        m_atRight = false;
                    }
                    if (vector3.y < 0f)
                    {
                        m_atBottom = false;
                    }
                    else if (vector3.y > 0f)
                    {
                        m_atTop = false;
                    }
                    if (m_atRight && vector3.x < 0f)
                    {
                        vector3.x = 0f;
                    }
                    else if (m_atLeft && vector3.x > 0f)
                    {
                        vector3.x = 0f;
                    }
                    if (m_atTop && vector3.y < 0f)
                    {
                        vector3.y = 0f;
                    }
                    else if (m_atBottom && vector3.y > 0f)
                    {
                        vector3.y = 0f;
                    }
                    if (vector3.x < 0f)
                    {
                        m_testRight = true;
                    }
                    else if (vector3.x > 0f)
                    {
                        m_testLeft = true;
                    }
                    if (vector3.y < 0f)
                    {
                        m_testTop = true;
                    }
                    else if (vector3.y > 0f)
                    {
                        m_testBottom = true;
                    }
                    position_offset += -main.transform.right * CameraPanDeltaX.magnitude * vector3.x;
                    position_offset += -main.transform.up * Vector3.Dot(-main.transform.up, CameraPanDeltaY) * vector3.y;
                }
                else
                {
                    bool option = GameState.Option.GetOption(GameOption.BoolOption.SCREEN_EDGE_SCROLLING);
                    bool flag = GameInput.MousePosition.x > 0f + m_mouseScrollBufferOuter && GameInput.MousePosition.x < (float)Screen.width - m_mouseScrollBufferOuter;
                    bool flag2 = GameInput.MousePosition.y > 0f + m_mouseScrollBufferOuter && GameInput.MousePosition.y < (float)Screen.height - m_mouseScrollBufferOuter;
                    if (GameInput.GetControl(MappedControl.PAN_CAMERA_LEFT) || (flag2 && option && GameInput.MousePosition.x < m_mouseScrollBuffer && GameInput.MousePosition.x > m_mouseScrollBufferOuter))
                    {
                        CancelFollow();
                        m_atRight = false;
                        if (!m_atLeft)
                        {
                            position_offset -= Camera.main.transform.right * CameraMoveDelta;
                            m_testLeft = true;
                        }
                    }
                    else if (GameInput.GetControl(MappedControl.PAN_CAMERA_RIGHT) || (flag2 && option && GameInput.MousePosition.x > (float)Screen.width - m_mouseScrollBuffer && GameInput.MousePosition.x < (float)Screen.width - m_mouseScrollBufferOuter))
                    {
                        CancelFollow();
                        m_atLeft = false;
                        if (!m_atRight)
                        {
                            position_offset += Camera.main.transform.right * CameraMoveDelta;
                            m_testRight = true;
                        }
                    }
                    if (GameInput.GetControl(MappedControl.PAN_CAMERA_DOWN) || (flag && option && GameInput.MousePosition.y < m_mouseScrollBuffer && GameInput.MousePosition.y > m_mouseScrollBufferOuter))
                    {
                        CancelFollow();
                        m_atTop = false;
                        if (!m_atBottom)
                        {
                            position_offset -= Camera.main.transform.up * CameraMoveDelta;
                            m_testBottom = true;
                        }
                    }
                    else if (GameInput.GetControl(MappedControl.PAN_CAMERA_UP) || (flag && option && GameInput.MousePosition.y > (float)Screen.height - m_mouseScrollBuffer && GameInput.MousePosition.y < (float)Screen.height - m_mouseScrollBufferOuter))
                    {
                        CancelFollow();
                        m_atBottom = false;
                        if (!m_atTop)
                        {
                            position_offset += Camera.main.transform.up * CameraMoveDelta;
                            m_testTop = true;
                        }
                    }
                }
            }
            if (m_FollowingUnits.Count > 0 && GetDeltaTime() > 0f && !InterpolatingToTarget)
            {
                Vector3 zero = Vector3.zero;
                int num3 = 0;
                for (int num4 = m_FollowingUnits.Count - 1; num4 >= 0; num4--)
                {
                    if ((bool)m_FollowingUnits[num4])
                    {
                        zero += m_FollowingUnits[num4].transform.position;
                        num3++;
                    }
                }
                if (num3 > 0)
                {
                    FocusOnPoint(zero / num3);
                }
            }
            if (InterpolatingToTarget)
            {
                if ((bool)GameState.s_playerCharacter)
                {
                    MoveTime -= GetDeltaTime();
                }
                Vector3 position = Vector3.zero;
                if (MoveTime <= 0f)
                {
                    MoveTime = 0f;
                    InterpolatingToTarget = false;
                    position = MoveToPointDest;
                }
                else
                {
                    float t = MoveTime / MoveTotalTime;
                    position.x = Mathf.SmoothStep(MoveToPointDest.x, MoveToPointSrc.x, t);
                    position.y = Mathf.SmoothStep(MoveToPointDest.y, MoveToPointSrc.y, t);
                    position.z = Mathf.SmoothStep(MoveToPointDest.z, MoveToPointSrc.z, t);
                }
                lastPosition = position;
                base.transform.position = position;
                position_offset = Vector3.zero;
                ResetAtEdges();
            }
            Vector3 zero2 = Vector3.zero;
            if (m_screenShakeTimer > 0f && GameState.s_playerCharacter != null)
            {
                m_screenShakeTimer -= Time.unscaledDeltaTime;
                Vector3 vector4 = Random.onUnitSphere * m_screenShakeStrength * (m_screenShakeTimer / m_screenShakeTotalTime);
                zero2 += vector4.x * -main.transform.right;
                zero2 += vector4.y * -main.transform.up;
            }
            Vector3 zero3 = Vector3.zero;
            base.transform.position = lastPosition + position_offset + zero2;
            if (!m_blockoutMode)
            {
                Vector3 vector5 = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, Camera.main.nearClipPlane));
                Vector3 lhs = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, Camera.main.nearClipPlane));
                Vector3 vector6 = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0f, Camera.main.nearClipPlane)) - vector5;
                Vector3 vector7 = Camera.main.ScreenToWorldPoint(new Vector3(0f, Camera.main.pixelHeight, Camera.main.nearClipPlane)) - vector5;
                float magnitude = vector6.magnitude;
                float magnitude2 = vector7.magnitude;
                vector5 -= m_worldBoundsOrigin;
                lhs -= m_worldBoundsOrigin;
                Vector3 worldBoundsX = m_worldBoundsX;
                Vector3 worldBoundsY = m_worldBoundsY;
                worldBoundsX.Normalize();
                worldBoundsY.Normalize();
                float num5 = Vector3.Dot(vector5, worldBoundsX);
                float num6 = Vector3.Dot(vector5, worldBoundsY);
                float num7 = Vector3.Dot(lhs, worldBoundsX);
                float num8 = Vector3.Dot(lhs, worldBoundsY);
                float magnitude3 = m_worldBoundsX.magnitude;
                float magnitude4 = m_worldBoundsY.magnitude;
                float num9 = BufferLeft * magnitude;
                float num10 = BufferRight * magnitude;
                float num11 = BufferTop * magnitude2;
                float num12 = BufferBottom * magnitude2;
                if (magnitude > magnitude3)
                {
                    float num13 = (magnitude - magnitude3) / 2f;
                    num9 += num13;
                    num10 += num13;
                }
                if (magnitude2 > magnitude4)
                {
                    float num14 = (magnitude2 - magnitude4) / 2f;
                    num11 += num14;
                    num12 += num14;
                }
                if (m_testLeft && num5 < 0f - num9)
                {
                    zero3 += (0f - num5 - num9) * worldBoundsX;
                    m_atLeft = true;
                    m_atRight = false;
                }
                else if (m_testRight && num7 > magnitude3 + num10)
                {
                    zero3 -= (num7 - (magnitude3 + num10)) * worldBoundsX;
                    m_atRight = true;
                    m_atLeft = false;
                }
                if (m_testBottom && num6 < 0f - num12)
                {
                    zero3 += (0f - num6 - num12) * worldBoundsY;
                    m_atBottom = true;
                    m_atTop = false;
                }
                else if (m_testTop && num8 > magnitude4 + num11)
                {
                    zero3 -= (num8 - (magnitude4 + num11)) * worldBoundsY;
                    m_atTop = true;
                    m_atBottom = false;
                }
                base.transform.position = lastPosition + position_offset + zero3;
                lastPosition = base.transform.position;
                base.transform.position += zero2;
                position_offset = Vector3.zero;
            }
            if (Audio != null)
            {
                Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                int layerMask = 1 << LayerMask.NameToLayer("Walkable");
                if (Physics.Raycast(ray, out var hitInfo, float.PositiveInfinity, layerMask))
                {
                    Audio.position = hitInfo.point;
                    m_lastAudioY = Audio.position.y;
                }
                else
                {
                    Plane cameraPlane = new Plane(Vector3.up, new Vector3(0f, m_lastAudioY, 0f));
                    Audio.position = GetPlaneRayIntersectionPosition(cameraPlane, ray);
                }
            }
        }
    }

    [ModifiesType]
    class V1ld_UIAreaMap : UIAreaMap
    {
        [ModifiesMember("FocusCameraOnPointer")]
        new public void FocusCameraOnPointer()
        {
            if ((bool)m_uiCamera)
            {
                Vector3 v = m_uiCamera.ScreenToWorldPoint(GameInput.MousePosition);
                Vector3 vector = base.transform.worldToLocalMatrix.MultiplyPoint3x4(v);
                CameraControl instance = CameraControl.Instance;
                LevelInfo instance2 = LevelInfo.Instance;
                Vector3 backgroundQuadOrigin = instance2.m_backgroundQuadOrigin;
                Vector3 vector2 = instance2.m_backgroundQuadAxisX * instance2.m_backgroundQuadWidth;
                Vector3 vector3 = instance2.m_backgroundQuadAxisY * instance2.m_backgroundQuadHeight;
                Vector3 point = backgroundQuadOrigin + vector2 * (vector.x / UIAreaMapManager.Instance.AreaMapWidthFillPercentage + 0.5f) + vector3 * (vector.y / UIAreaMapManager.Instance.AreaMapHeightFillPercentage + 0.5f);
                // v1ld: fix vanilla bug where follow mode bugged out right/double click in map mode
                instance.CancelFollow();
                instance.FocusOnPoint(point);
                instance.DoUpdate();
            }
        }
    }
}